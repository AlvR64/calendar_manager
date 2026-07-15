# Availability

## Roadmap IDs

- `5.1`
- `5.2`
- `5.3`
- `5.4`
- `5.5`
- `5.6`
- `5.7`
- `5.8`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `5.1` | [x] | [x] | [x] | [ ] |
| `5.2` | [x] | [x] | [x] | [ ] |
| `5.3` | [x] | [x] | [x] | [ ] |
| `5.4` | [x] | [x] | [x] | [ ] |
| `5.5` | [x] | [x] | [x] | [ ] |
| `5.6` | [x] | [x] | [x] | [ ] |
| `5.7` | [x] | [x] | [x] | [ ] |
| `5.8` | [x] | [x] | [x] | [ ] |

## Goal

Implement admin availability management so admins can define weekly availability and date-specific exceptions for each staff member.

## Scope

- Availability page with staff member selection.
- Weekly availability list/create/update/delete.
- Availability exceptions list/create/update/delete.
- Closed day exceptions and special-hour exceptions.
- Conflict/overlap validation display.
- Timezone context from business settings where available.

## Non-Goals

- Creating appointments.
- Calendar drag/drop UI unless already represented by design.
- Staff login.
- External calendar sync.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/staff-members` | Admin JWT | None | `200 StaffMemberResponse[]` | `401`, `403`, `404` |
| `GET /api/staff-members/{staffMemberId}/availability` | Admin JWT | Route id | `200 StaffMemberAvailabilityResponse[]` | `401`, `403`, `404` |
| `POST /api/staff-members/{staffMemberId}/availability` | Admin JWT | `CreateStaffMemberAvailabilityRequest` | `201 StaffMemberAvailabilityResponse` | `400`, `401`, `403`, `404`, `409` |
| `PUT /api/staff-members/{staffMemberId}/availability/{availabilityId}` | Admin JWT | `UpdateStaffMemberAvailabilityRequest` | `200 StaffMemberAvailabilityResponse` | `400`, `401`, `403`, `404`, `409` |
| `DELETE /api/staff-members/{staffMemberId}/availability/{availabilityId}` | Admin JWT | Route ids | `204` | `401`, `403`, `404` |
| `GET /api/staff-members/{staffMemberId}/availability-exceptions` | Admin JWT | Route id | `200 StaffMemberAvailabilityExceptionResponse[]` | `401`, `403`, `404` |
| `POST /api/staff-members/{staffMemberId}/availability-exceptions` | Admin JWT | `CreateStaffMemberAvailabilityExceptionRequest` | `201 StaffMemberAvailabilityExceptionResponse` | `400`, `401`, `403`, `404`, `409` |
| `PUT /api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}` | Admin JWT | `UpdateStaffMemberAvailabilityExceptionRequest` | `200 StaffMemberAvailabilityExceptionResponse` | `400`, `401`, `403`, `404`, `409` |
| `DELETE /api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}` | Admin JWT | Route ids | `204` | `401`, `403`, `404` |

## Design References

- `designs/admin-availability.op`

## Frontend Routes And Screens

- `/admin/availability`

## UX States

- Loading staff members.
- Empty state when no staff members exist.
- Loading availability for selected staff member.
- Empty weekly availability state.
- Empty exceptions state.
- Validation errors for invalid day/time ranges.
- Conflict errors for overlaps or duplicate exceptions.
- Success states after create/update/delete.

## Data And Validation Rules

- Availability is per `StaffMember`.
- `DayOfWeek` uses backend integer convention.
- `StartTime` must be before `EndTime`.
- Closed exceptions should not send start/end time.
- Special-hour exceptions should include start/end time.
- Dates/times are local to the business timezone.
- Backend validation remains authoritative.

## Implementation Plan

1. Add availability and exception TypeScript contracts.
2. Add API functions and query/mutation hooks.
3. Load staff members and select a staff member.
4. Implement weekly availability UI and forms.
5. Implement exception UI and forms.
6. Show timezone context and validation/conflict errors.
7. Invalidate selected staff availability queries after mutations.
8. Add tests for staff selection, weekly CRUD, exception CRUD, and conflict rendering.
9. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: staff selection, availability list, exception list, validation, conflict errors.
- Integration/manual checks: availability CRUD and exception CRUD against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] `/admin/availability` is not a placeholder.
- [ ] Admin can manage weekly availability for a selected staff member.
- [ ] Admin can manage closed and special-hour exceptions.
- [ ] Overlap/duplicate/conflict errors are shown clearly.
- [ ] UI follows `designs/admin-availability.op`.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for `5.1` through `5.8`.

## Open Questions

- Resolved: `5.11` and `5.12` stay unmarked in frontend for this slice. They should be completed by scheduling/slot/appointment flows rather than the admin availability editor.
