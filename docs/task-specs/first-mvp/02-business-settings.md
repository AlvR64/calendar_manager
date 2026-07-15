# Business Settings

## Roadmap IDs

- `0.11`
- `1.2`
- `1.6`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `0.11` | [ ] | [ ] | [x] | [ ] |
| `1.2` | [x] | [ ] | [x] | [ ] |
| `1.6` | [x] | [ ] | [x] | [ ] |

## Goal

Implement the admin shell and business settings screen so an authenticated admin can view and update public business details and booking window settings.

## Scope

- Admin layout/navigation sufficient for first MVP admin pages.
- Business settings screen.
- Load current business data for the authenticated admin.
- Update public details, contact, address, time zone, and currency.
- Update `maxAdvanceBookingDays`.
- Loading, success, validation, unauthorized, and error states.

## Non-Goals

- Business activation/deactivation.
- Cancellation policy.
- Minimum booking notice.
- Appointment buffers.
- Multi-admin permissions.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/businesses/{businessId}` | None currently | Route `businessId` from admin session | `200 BusinessResponse` | `404` |
| `PUT /api/businesses/current` | Admin JWT | `UpdateBusinessDetailsRequest` | `200 BusinessResponse` | `400`, `401`, `403`, `404` |
| `PUT /api/businesses/current/booking-window` | Admin JWT | `UpdateBusinessBookingWindowRequest` | `200 BusinessBookingWindowResponse` | `400`, `401`, `403`, `404` |

## Design References

- `designs/business-admin-dashboard.op`
- `designs/business-settings.op`

## Frontend Routes And Screens

- `/admin`
- `/admin/business-settings`

## UX States

- Loading while reading business details.
- Loading while saving details or booking window.
- Validation errors.
- Success confirmation after save.
- Unauthorized redirect to admin login.
- Error state for not found or failed save.

## Data And Validation Rules

- Name is required.
- Email fields use email validation when present.
- Website URL should be valid URL when present.
- Country code and currency code should match backend constraints.
- Time zone is an IANA id; backend remains authoritative.
- `maxAdvanceBookingDays` should be positive and match backend range constraints.

## Implementation Plan

1. Add missing business request/response TypeScript contracts.
2. Add business API functions for load/update details/update booking window.
3. Reuse admin auth session to get token and business id.
4. Implement admin shell according to first MVP design.
5. Implement business settings forms with React Hook Form + Zod.
6. Add mutations and query invalidation.
7. Cover loading, validation, error, and success states.
8. Add tests for rendering loaded values and submitting updates.
9. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: loaded values appear, validation errors, save success/error.
- Integration/manual checks: update details and booking window against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] `/admin/business-settings` loads real business data.
- [ ] Details update uses `PUT /api/businesses/current`.
- [ ] Booking window update uses `PUT /api/businesses/current/booking-window`.
- [ ] Admin shell is usable for first MVP admin navigation.
- [ ] Unauthorized admins are redirected or blocked correctly.
- [ ] UI follows referenced OpenPencil designs.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for completed frontend IDs.

## Open Questions

- Should backend add `GET /api/businesses/current` for a cleaner admin settings load instead of using public `GET /api/businesses/{businessId}` with the admin session business id?
