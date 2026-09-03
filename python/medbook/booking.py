from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime
from threading import Barrier, Lock
from typing import Optional, Protocol
from uuid import uuid4


@dataclass(frozen=True)
class BookRequest:
    request_id: str
    tenant_id: str
    patient_id: str
    provider_id: str
    start_time: datetime


@dataclass(frozen=True)
class Appointment:
    id: str
    request_id: str
    tenant_id: str
    patient_id: str
    provider_id: str
    start_time: datetime
    status: str = "CONFIRMED"


class SlotUnavailableError(Exception):
    pass


class IdempotencyConflictError(Exception):
    pass


class IntegrationPublisher(Protocol):
    def publish_calendar_event(self, appointment: Appointment) -> None: ...
    def publish_confirmation(self, appointment: Appointment) -> None: ...


class NoOpIntegrationPublisher:
    def publish_calendar_event(self, appointment: Appointment) -> None:
        return None

    def publish_confirmation(self, appointment: Appointment) -> None:
        return None


class InMemoryAppointmentRepository:
    def __init__(self, race_barrier: Optional[Barrier] = None):
        self._by_request: dict[tuple[str, str], Appointment] = {}
        self._lock = Lock()
        self._race_barrier = race_barrier

    def find_by_request_id(self, tenant_id: str, request_id: str) -> Optional[Appointment]:
        with self._lock:
            return self._by_request.get((tenant_id, request_id))

    def is_slot_booked(self, tenant_id: str, provider_id: str, start_time: datetime) -> bool:
        with self._lock:
            booked = any(
                item.tenant_id == tenant_id
                and item.provider_id == provider_id
                and item.start_time == start_time
                for item in self._by_request.values()
            )
        if self._race_barrier is not None:
            self._race_barrier.wait(timeout=5)
        return booked

    def insert(self, appointment: Appointment) -> None:
        with self._lock:
            self._by_request[(appointment.tenant_id, appointment.request_id)] = appointment


class BookingService:
    def __init__(self, repository: InMemoryAppointmentRepository, integrations: IntegrationPublisher):
        self._repository = repository
        self._integrations = integrations

    def book(self, request: BookRequest) -> Appointment:
        existing = self._repository.find_by_request_id(request.tenant_id, request.request_id)
        if existing is not None:
            return existing

        if self._repository.is_slot_booked(
            request.tenant_id, request.provider_id, request.start_time
        ):
            raise SlotUnavailableError("The provider slot is unavailable.")

        appointment = Appointment(
            id=uuid4().hex,
            request_id=request.request_id,
            tenant_id=request.tenant_id,
            patient_id=request.patient_id,
            provider_id=request.provider_id,
            start_time=request.start_time,
        )
        self._repository.insert(appointment)
        self._integrations.publish_calendar_event(appointment)
        self._integrations.publish_confirmation(appointment)
        return appointment
