# Staff-Service Assignments

## Roadmap IDs

- `4.1`
- `4.2`
- `4.3`
- `4.4`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `4.1` | [x] | [ ] | [x] | [ ] |
| `4.2` | [x] | [ ] | [x] | [ ] |
| `4.3` | [x] | [ ] | [x] | [ ] |
| `4.4` | [x] | [ ] | [x] | [ ] |

## Goal

Implement the admin UI needed to connect services with staff members so availability and public slot selection can reflect who can perform each service.

## Scope

- Assignment controls in admin services and/or staff members screens.
- Assign service to staff member.
- Assign staff member to service.
- Activate/deactivate an assignment.
- Unassign service from staff member.
- Conflict, not found, loading, and success states.

## Non-Goals

- Custom duration per staff-service.
- Custom price per staff-service.
- Dedicated assignment list endpoints unless required by the implementation decision below.
- Appointment handling for assignments.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `POST /api/staff-members/{staffMemberId}/services/{serviceId}` | Admin JWT | Route ids | `201 StaffMemberServiceAssignmentResponse` | `400`, `401`, `403`, `404`, `409` |
| `POST /api/services/{serviceId}/staff-members/{staffMemberId}` | Admin JWT | Route ids | `201 StaffMemberServiceAssignmentResponse` | `400`, `401`, `403`, `404`, `409` |
| `PUT /api/staff-members/{staffMemberId}/services/{serviceId}/active-state` | Admin JWT | `UpdateStaffMemberServiceActiveStateRequest` | `200 StaffMemberServiceAssignmentResponse` | `400`, `401`, `403`, `404` |
| `DELETE /api/staff-members/{staffMemberId}/services/{serviceId}` | Admin JWT | Route ids | `204` | `401`, `403`, `404`, `409` |

## Design References

- `designs/admin-services.op`
- `designs/admin-staff-members.op`

## Frontend Routes And Screens

- `/admin/services`
- `/admin/staff-members`

## UX States

- Loading available services/staff members.
- Assignment mutation loading states.
- Conflict when assignment already exists.
- Conflict when unassign is blocked by appointments.
- Empty state when no services or staff members exist.
- Success state after assignment changes.

## Data And Validation Rules

- Staff member id and service id must be selected before assignment.
- Backend validation and ownership checks remain authoritative.
- Inactive services/staff members may appear in admin lists; UI should make active state clear.

## Implementation Plan

1. Verify whether current admin service/staff responses provide enough assignment state for the designed UI.
2. If assignment state is insufficient, decide whether to add backend list endpoints for roadmap `4.5`/`4.6` or limit first MVP UI scope.
3. Add assignment TypeScript contracts and API functions.
4. Add hooks/mutations for assign, toggle active state, and unassign.
5. Integrate assignment UI into services and/or staff members pages.
6. Handle conflicts and not-found errors explicitly.
7. Add tests for assignment, duplicate conflict, active toggle, and unassign conflict.
8. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: assignment controls render, required selection, mutation success/error paths.
- Integration/manual checks: assign/toggle/unassign against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] Admin can assign a service to a staff member from a designed admin workflow.
- [ ] Admin can assign a staff member to a service from a designed admin workflow, or the spec documents one canonical direction for MVP.
- [ ] Admin can activate/deactivate an assignment.
- [ ] Admin can unassign and sees conflicts clearly.
- [ ] UI follows referenced OpenPencil designs.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for completed frontend IDs.

## Open Questions

- Do we need to move roadmap `4.5` and `4.6` out of postpuesto and add admin list endpoints so the UI can show current assignments accurately, including inactive assignments?
