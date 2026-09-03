namespace MedBook.Booking;

public sealed record BookRequest(
    string RequestId,
    string TenantId,
    string PatientId,
    string ProviderId,
    DateTimeOffset StartTime);

public sealed record Appointment(
    string Id,
    string RequestId,
    string TenantId,
    string PatientId,
    string ProviderId,
    DateTimeOffset StartTime,
    string Status)
{
    public static Appointment Create(BookRequest request) => new(
        Guid.NewGuid().ToString("N"),
        request.RequestId,
        request.TenantId,
        request.PatientId,
        request.ProviderId,
        request.StartTime,
        "CONFIRMED");
}

public sealed class SlotUnavailableException : Exception
{
    public SlotUnavailableException() : base("The provider slot is unavailable.") { }
}

public sealed class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException() : base("The request ID was reused with different booking data.") { }
}
