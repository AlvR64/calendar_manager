# Services

## Roadmap IDs

- `2.1`
- `2.2`
- `2.3`
- `2.4`
- `2.5`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `2.1` | [x] | [ ] | [x] | [ ] |
| `2.2` | [x] | [ ] | [x] | [ ] |
| `2.3` | [x] | [ ] | [x] | [ ] |
| `2.4` | [x] | [ ] | [x] | [ ] |
| `2.5` | [x] | [ ] | [x] | [ ] |

## Goal

Implement admin service management so an authenticated admin can create, view, update, activate/deactivate, and delete services for their business.

## Scope

- Admin services page.
- List active and inactive services.
- Create service.
- Edit service.
- Activate/deactivate service.
- Delete service and show conflict errors.
- Empty, loading, validation, success, and error states.

## Non-Goals

- Service images.
- Staff-service assignments UI; covered by `05-staff-service-assignments.md`.
- Public service detail page beyond data consumed by public profile/slot flow specs.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/services` | Admin JWT | None | `200 ServiceResponse[]` | `401`, `403`, `404` |
| `GET /api/services/{serviceId}` | Admin JWT | Route id | `200 ServiceResponse` | `401`, `403`, `404` |
| `POST /api/services` | Admin JWT | `CreateServiceRequest` | `201 ServiceResponse` | `400`, `401`, `403`, `404` |
| `PUT /api/services/{serviceId}` | Admin JWT | `UpdateServiceRequest` | `200 ServiceResponse` | `400`, `401`, `403`, `404` |
| `PUT /api/services/{serviceId}/active-state` | Admin JWT | `UpdateServiceActiveStateRequest` | `200 ServiceResponse` | `400`, `401`, `403`, `404` |
| `DELETE /api/services/{serviceId}` | Admin JWT | Route id | `204` | `401`, `403`, `404`, `409` |

## Design References

- `designs/admin-services.op`

## Frontend Routes And Screens

- `/admin/services`

## UX States

- Loading service list.
- Empty state when no services exist.
- Create/edit form validation.
- Saving and deleting loading states.
- Delete confirmation.
- Conflict state when delete is blocked by appointments.
- Generic API error state.

## Data And Validation Rules

- Name is required.
- Duration must be positive minutes.
- Price amount must be non-negative.
- Sort order must be numeric.
- Backend validation remains authoritative.

## Implementation Plan

1. Add service request TypeScript contracts if missing.
2. Add service API functions and TanStack Query hooks.
3. Implement services list UI from design.
4. Implement create/edit form.
5. Implement active-state toggle.
6. Implement delete confirmation and conflict rendering.
7. Invalidate services query after mutations.
8. Add tests for list, create/edit validation, active toggle, and delete conflict.
9. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: empty/list render, form validation, mutation success/error paths.
- Integration/manual checks: CRUD service against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] `/admin/services` is not a placeholder.
- [ ] Services are loaded from `GET /api/services`.
- [ ] Create, edit, activate/deactivate, and delete call real backend endpoints.
- [ ] Delete conflicts are shown clearly.
- [ ] UI follows `designs/admin-services.op`.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for `2.1` through `2.5`.

## Open Questions

- None.
