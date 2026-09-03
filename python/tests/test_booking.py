import sys
import unittest
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from pathlib import Path
from threading import Barrier

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from medbook import (  # noqa: E402
    BookRequest,
    BookingService,
    IdempotencyConflictError,
    InMemoryAppointmentRepository,
    NoOpIntegrationPublisher,
    SlotUnavailableError,
)


def request(request_id: str, patient_id: str = "patient-1") -> BookRequest:
    return BookRequest(
        request_id=request_id,
        tenant_id="clinic-1",
        patient_id=patient_id,
        provider_id="doctor-1",
        start_time=datetime(2026, 9, 14, 14, 0, tzinfo=timezone.utc),
    )


class BookingTests(unittest.TestCase):
    def service(self, repository):
        return BookingService(repository, NoOpIntegrationPublisher())

    def test_same_request_is_idempotent(self):
        service = self.service(InMemoryAppointmentRepository())
        first = service.book(request("request-1"))
        second = service.book(request("request-1"))
        self.assertEqual(first.id, second.id)

    def test_reused_request_id_with_different_payload_is_rejected(self):
        service = self.service(InMemoryAppointmentRepository())
        service.book(request("request-2", "patient-1"))
        with self.assertRaises(IdempotencyConflictError):
            service.book(request("request-2", "patient-2"))

    def test_concurrent_requests_cannot_double_book(self):
        service = self.service(InMemoryAppointmentRepository(Barrier(2)))

        def capture(item):
            try:
                return service.book(item)
            except Exception as exc:  # the result is classified below
                return exc

        with ThreadPoolExecutor(max_workers=2) as pool:
            results = list(pool.map(capture, [
                request("request-a", "patient-a"),
                request("request-b", "patient-b"),
            ]))

        successes = sum(not isinstance(result, Exception) for result in results)
        conflicts = sum(isinstance(result, SlotUnavailableError) for result in results)
        self.assertEqual((successes, conflicts), (1, 1))


if __name__ == "__main__":
    unittest.main()
