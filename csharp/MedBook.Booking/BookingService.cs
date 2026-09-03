namespace MedBook.Booking;

public sealed class BookingService
{
    private readonly IAppointmentRepository _repository;
    private readonly IIntegrationPublisher _integrations;

    public BookingService(IAppointmentRepository repository, IIntegrationPublisher integrations)
    {
        _repository = repository;
        _integrations = integrations;
    }

    public async Task<Appointment> BookAsync(
        BookRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.FindByRequestIdAsync(
            request.TenantId,
            request.RequestId,
            cancellationToken);

        if (existing is not null)
            return existing;

        if (await _repository.IsSlotBookedAsync(
            request.TenantId,
            request.ProviderId,
            request.StartTime,
            cancellationToken))
            throw new SlotUnavailableException();

        var appointment = Appointment.Create(request);
        await _repository.InsertAsync(appointment, cancellationToken);

        await _integrations.PublishCalendarEventAsync(appointment, cancellationToken);
        await _integrations.PublishConfirmationAsync(appointment, cancellationToken);

        return appointment;
    }
}
