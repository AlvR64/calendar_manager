# Staff Members

## Roadmap IDs

- `3.1`
- `3.2`
- `3.3`
- `3.4`
- `3.5`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `3.1` | [x] | [ ] | [x] | [ ] |
| `3.2` | [x] | [ ] | [x] | [ ] |
| `3.3` | [x] | [ ] | [x] | [ ] |
| `3.4` | [x] | [ ] | [x] | [ ] |
| `3.5` | [x] | [ ] | [x] | [ ] |

## Goal

Implement admin staff member management so an authenticated admin can manage the people/resources that provide services and availability.

## Scope

- Admin staff members page.
- List active and inactive staff members.
- Create staff member.
- Edit staff member.
- Activate/deactivate staff member.
- Delete staff member and show conflict errors.
- Empty, loading, validation, success, and error states.

## Non-Goals

- Staff login.
- Staff avatar/photo upload.
- Staff-service assignment UI; covered by `05-staff-service-assignments.md`.
- Availability UI; covered by `06-availability.md`.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/staff-members` | Admin JWT | None | `200 StaffMemberResponse[]` | `401`, `403`, `404` |
| `GET /api/staff-members/{staffMemberId}` | Admin JWT | Route id | `200 StaffMemberResponse` | `401`, `403`, `404` |
| `POST /api/staff-members` | Admin JWT | `CreateStaffMemberRequest` | `201 StaffMemberResponse` | `400`, `401`, `403`, `404` |
| `PUT /api/staff-members/{staffMemberId}` | Admin JWT | `UpdateStaffMemberRequest` | `200 StaffMemberResponse` | `400`, `401`, `403`, `404` |
| `PUT /api/staff-members/{staffMemberId}/active-state` | Admin JWT | `UpdateStaffMemberActiveStateRequest` | `200 StaffMemberResponse` | `400`, `401`, `403`, `404` |
| `DELETE /api/staff-members/{staffMemberId}` | Admin JWT | Route id | `204` | `401`, `403`, `404`, `409` |

## Design References

- `designs/admin-staff-members.op`

## Frontend Routes And Screens

- `/admin/staff-members`

## UX States

- Loading staff member list.
- Empty state when no staff members exist.
- Create/edit form validation.
- Saving and deleting loading states.
- Delete confirmation.
- Conflict state when delete is blocked by appointments.
- Generic API error state.

## Data And Validation Rules

- Display name is required.
- Email uses email validation when present.
- Phone number is optional.
- Bio is optional and should respect backend length constraints.
- Sort order must be numeric.
- Backend validation remains authoritative.

## Implementation Plan

1. Add staff member request TypeScript contracts if missing.
2. Add staff member API functions and TanStack Query hooks.
3. Implement staff members list UI from design.
4. Implement create/edit form.
5. Implement active-state toggle.
6. Implement delete confirmation and conflict rendering.
7. Invalidate staff members query after mutations.
8. Add tests for list, create/edit validation, active toggle, and delete conflict.
9. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: empty/list render, form validation, mutation success/error paths.
- Integration/manual checks: CRUD staff member against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] `/admin/staff-members` is not a placeholder.
- [ ] Staff members are loaded from `GET /api/staff-members`.
- [ ] Create, edit, activate/deactivate, and delete call real backend endpoints.
- [ ] Delete conflicts are shown clearly.
- [ ] UI follows `designs/admin-staff-members.op`.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for `3.1` through `3.5`.

## Open Questions

- None.
