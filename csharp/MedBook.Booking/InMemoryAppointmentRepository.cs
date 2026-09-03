using System.Collections.Concurrent;

namespace MedBook.Booking;

public sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly ConcurrentDictionary<(string TenantId, string RequestId), Appointment> _byRequest = new();
    private readonly Barrier? _raceBarrier;

    public InMemoryAppointmentRepository(Barrier? raceBarrier = null)
    {
        _raceBarrier = raceBarrier;
    }

    public Task<Appointment?> FindByRequestIdAsync(
        string tenantId,
        string requestId,
        CancellationToken cancellationToken)
    {
        _byRequest.TryGetValue((tenantId, requestId), out var appointment);
        return Task.FromResult(appointment);
    }

    public Task<bool> IsSlotBookedAsync(
        string tenantId,
        string providerId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken)
    {
        var booked = _byRequest.Values.Any(a =>
            a.TenantId == tenantId &&
            a.ProviderId == providerId &&
            a.StartTime == startTime);

        _raceBarrier?.SignalAndWait(cancellationToken);
        return Task.FromResult(booked);
    }

    public Task InsertAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _byRequest[(appointment.TenantId, appointment.RequestId)] = appointment;
        return Task.CompletedTask;
    }
}
