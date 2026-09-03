using MedBook.Booking;

var tests = new (string Name, Func<Task> Run)[]
{
    ("same request is idempotent", SameRequestIsIdempotent),
    ("request ID reuse with different payload is rejected", ReusedRequestIdIsRejected),
    ("concurrent requests cannot double-book a slot", ConcurrentRequestsCannotDoubleBook)
};

var failures = 0;
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS  {test.Name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL  {test.Name}");
        Console.WriteLine($"      {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine($"\n{tests.Length - failures} passed; {failures} failed");
return failures == 0 ? 0 : 1;

static BookRequest Request(string requestId, string patientId = "patient-1") => new(
    requestId,
    "clinic-1",
    patientId,
    "doctor-1",
    DateTimeOffset.Parse("2026-09-14T14:00:00Z"));

static BookingService Service(IAppointmentRepository repository) =>
    new(repository, new NoOpIntegrationPublisher());

static async Task SameRequestIsIdempotent()
{
    var service = Service(new InMemoryAppointmentRepository());
    var first = await service.BookAsync(Request("request-1"));
    var second = await service.BookAsync(Request("request-1"));
    Assert(first.Id == second.Id, "The retry returned a different appointment.");
}

static async Task ReusedRequestIdIsRejected()
{
    var service = Service(new InMemoryAppointmentRepository());
    await service.BookAsync(Request("request-2", "patient-1"));
    await AssertThrows<IdempotencyConflictException>(
        () => service.BookAsync(Request("request-2", "patient-2")));
}

static async Task ConcurrentRequestsCannotDoubleBook()
{
    using var barrier = new Barrier(2);
    var service = Service(new InMemoryAppointmentRepository(barrier));
    var attempts = new[]
    {
        service.BookAsync(Request("request-a", "patient-a")),
        service.BookAsync(Request("request-b", "patient-b"))
    };

    var results = await Task.WhenAll(attempts.Select(Capture));
    var successes = results.Count(r => r is Appointment);
    var conflicts = results.Count(r => r is SlotUnavailableException);
    Assert(successes == 1 && conflicts == 1,
        $"Expected one booking and one slot conflict; observed {successes} bookings and {conflicts} conflicts.");
}

static async Task<object> Capture(Task<Appointment> task)
{
    try { return await task; }
    catch (Exception ex) { return ex; }
}

static async Task AssertThrows<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
