# First MVP Task Specs

The first MVP specs exist to remove backend/frontend/design desynchronization.

The immediate goal is to complete frontend features for the backend capabilities already implemented and the OpenPencil designs already created. New backend capabilities should wait until the first MVP frontend gap is closed, unless explicitly reprioritized.

`../../../task-groups-roadmap.md` remains the source of truth for status. Specs in this folder are implementation guides only.

## Scope

Included:

- Frontend shell, public landing, and admin shell behavior.
- Customer and admin auth screens backed by existing backend auth.
- Business registration and settings.
- Admin services.
- Admin staff members.
- Staff-service assignments.
- Availability and exceptions.
- Public business profile.
- Public slot flow up to selecting an appointment slot.

Excluded for now:

- Creating appointments.
- Listing appointments.
- Cancelling or rescheduling appointments.
- Dashboard/reporting metrics.
- Notifications.
- Media uploads.
- External calendar integrations.

## Specs

- `01-auth-and-registration.md`
- `02-business-settings.md`
- `03-services.md`
- `04-staff-members.md`
- `05-staff-service-assignments.md`
- `06-availability.md`
- `07-public-business-profile.md`
- `08-public-slot-flow.md`

Update each spec as needed before implementing that slice. Create additional specs from `../TEMPLATE.md` only when new first MVP slices are added.

## Completion Rule

A first MVP slice is complete only when:

- The relevant frontend UI is implemented beyond placeholders.
- It uses the backend contract or intentionally documented mock/dev state.
- Loading, empty, error, validation, and success states are covered where applicable.
- Relevant checks pass.
- `task-groups-roadmap.md` is updated.
