namespace MedBook.Booking;

public interface IAppointmentRepository
{
    Task<Appointment?> FindByRequestIdAsync(
        string tenantId,
        string requestId,
        CancellationToken cancellationToken);

    Task<bool> IsSlotBookedAsync(
        string tenantId,
        string providerId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken);

    Task InsertAsync(Appointment appointment, CancellationToken cancellationToken);
}
