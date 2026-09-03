# MedBook Booking Rules

The booking service must protect these business rules:

1. Within one tenant, only one appointment may occupy a provider's start time.
2. Different tenants may use the same provider identifier and start time without conflict.
3. A request ID is an idempotency key within a tenant.
4. Repeating the same request ID with the same booking payload returns the original appointment.
5. Reusing the same request ID with a different patient, provider, or start time is a conflict.
6. Calendar and notification providers must not determine whether the authoritative MedBook booking succeeds.
7. A committed booking must remain discoverable even if the caller disconnects or an integration is unavailable.

The starter implementation does not satisfy all of these rules.
