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

This table is a quick execution index. `../../../task-groups-roadmap.md` remains authoritative for backend, frontend, and design status.

| Spec | Status | Roadmap IDs | Notes |
| --- | --- | --- | --- |
| `01-auth-and-registration.md` | Done | `1.1`, `7.1`, `7.2`, `8.1` | Frontend implemented and roadmap updated. |
| `02-business-settings.md` | Done | `0.11`, `1.2`, `1.6` | Frontend implemented and roadmap updated. |
| `03-services.md` | Done | `2.1`-`2.5` | Frontend implemented and roadmap updated. |
| `04-staff-members.md` | Done | `3.1`-`3.5` | Frontend implemented and roadmap updated. |
| `05-staff-service-assignments.md` | Done | `4.1`-`4.6` | Frontend implemented; admin assignment listing endpoints added and roadmap updated. |
| `06-availability.md` | Done | `5.1`-`5.8` | Frontend implemented and roadmap updated; `5.11`/`5.12` remain for scheduling flows. |
| `07-public-business-profile.md` | Done | `0.10`, `11.1`-`11.7` | Frontend implemented and roadmap updated. |
| `08-public-slot-flow.md` | Done | `0.12`, `5.9`, `5.10`, `5.11`, `11.3`, `11.4`, `11.6` | Frontend implemented; stops before appointment creation. |

Update each spec as needed before implementing that slice. Create additional specs from `../TEMPLATE.md` only when new first MVP slices are added.

## Completion Rule

A first MVP slice is complete only when:

- The relevant frontend UI is implemented beyond placeholders.
- It uses the backend contract or intentionally documented mock/dev state.
- Loading, empty, error, validation, and success states are covered where applicable.
- Relevant checks pass.
- `task-groups-roadmap.md` is updated.
