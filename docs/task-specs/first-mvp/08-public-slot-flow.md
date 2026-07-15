# Public Slot Flow

## Roadmap IDs

- `0.12`
- `5.9`
- `5.10`
- `5.11`
- `11.3`
- `11.4`
- `11.6`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `0.12` | [ ] | [ ] | [x] | [ ] |
| `5.9` | [x] | [ ] | [x] | [ ] |
| `5.10` | [x] | [ ] | [x] | [ ] |
| `5.11` | [x] | [ ] | [x] | [ ] |
| `11.3` | [x] | [ ] | [x] | [ ] |
| `11.4` | [x] | [ ] | [x] | [ ] |
| `11.6` | [x] | [ ] | [x] | [ ] |

## Goal

Implement the public appointment slot selection flow up to selecting a slot, using backend-provided availability data without creating an appointment.

## Scope

- Public route `/b/:slug/appointment`.
- Load business profile by slug for context.
- Select active service.
- Optionally select staff member.
- Select date within booking window.
- Load available slots for service/date with optional staff member filter.
- Display backend-provided local and UTC slot data.
- Allow local slot selection.
- Loading, empty, error, and outside-window states.

## Non-Goals

- Creating appointments.
- Customer checkout/confirmation.
- Payment.
- Appointment cancellation/rescheduling.
- Slot holds.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/businesses/by-slug/{slug}/profile` | None | Slug route parameter | `200 BusinessProfileResponse` | `404` |
| `GET /api/businesses/{businessId}/services/{serviceId}/available-slots?date=YYYY-MM-DD` | None | Business id, service id, date query | `200 AvailableSlotResponse[]` | `400`, `404` |
| `GET /api/businesses/{businessId}/services/{serviceId}/staff-members/{staffMemberId}/available-slots?date=YYYY-MM-DD` | None | Business id, service id, staff id, date query | `200 AvailableSlotResponse[]` | `400`, `404` |

## Design References

- `designs/public-appointment-slot-flow.op`

## Frontend Routes And Screens

- `/b/:slug/appointment`

## UX States

- Loading business profile.
- Loading slots.
- No services available.
- No staff members available.
- No slots for selected date.
- Outside booking window.
- Invalid service/staff selection.
- Generic API error.
- Slot selected local state.

## Data And Validation Rules

- Business timezone is an IANA timezone id.
- Available slot responses include local and UTC times; prefer backend-provided slot data.
- Date selection must respect `business.maxAdvanceBookingDays`.
- Staff member filter is optional.
- Service must be selected before loading slots.
- Appointment creation is intentionally not submitted.

## Implementation Plan

1. Reuse public profile API/types from `07-public-business-profile.md`.
2. Add available slot API functions and query hooks.
3. Implement service selection.
4. Implement optional staff member selection, filtered by assignment data when available.
5. Implement date selection with booking window guard.
6. Load and render slots using backend-provided local times.
7. Store selected slot in local state and show selected summary.
8. Cover loading, empty, outside-window, not-found, and API error states.
9. Add tests for service/date selection and slot rendering.
10. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: service selection, optional staff selection, date guard, slot list, empty/error states.
- Integration/manual checks: load slots for real configured business/service/staff against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] `/b/:slug/appointment` is not a placeholder.
- [ ] Flow loads real business profile context.
- [ ] User can select service, optional staff member, date, and slot.
- [ ] Slots are fetched from real backend endpoints.
- [ ] Outside-window and no-slot states are clear.
- [ ] Flow stops after local slot selection and does not create an appointment.
- [ ] UI follows `designs/public-appointment-slot-flow.op`.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for completed frontend IDs.

## Open Questions

- Should the flow require customer login before slot selection, or only after appointment creation exists in a later backend slice?
