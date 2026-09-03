namespace MedBook.Booking;

public interface IIntegrationPublisher
{
    Task PublishCalendarEventAsync(Appointment appointment, CancellationToken cancellationToken);
    Task PublishConfirmationAsync(Appointment appointment, CancellationToken cancellationToken);
}

public sealed class NoOpIntegrationPublisher : IIntegrationPublisher
{
    public Task PublishCalendarEventAsync(Appointment appointment, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task PublishConfirmationAsync(Appointment appointment, CancellationToken cancellationToken) => Task.CompletedTask;
}
