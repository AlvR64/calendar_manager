# Public Business Profile

## Roadmap IDs

- `0.10`
- `11.1`
- `11.2`
- `11.3`
- `11.4`
- `11.5`
- `11.6`
- `11.7`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `0.10` | [ ] | [x] | [x] | [ ] |
| `11.1` | [x] | [x] | [x] | [ ] |
| `11.2` | [x] | [x] | [x] | [ ] |
| `11.3` | [x] | [x] | [x] | [ ] |
| `11.4` | [x] | [x] | [x] | [ ] |
| `11.5` | [x] | [x] | [x] | [ ] |
| `11.6` | [x] | [x] | [x] | [ ] |
| `11.7` | [x] | [x] | [x] | [ ] |

## Goal

Implement public discovery and business profile pages so customers can view a business, its active services, and active staff members before selecting an appointment slot.

## Scope

- Public homepage/landing from existing design.
- Public business profile route by slug.
- Load public profile data from backend.
- Display business details, active services, active staff members, and service/staff assignment context where useful.
- CTA to public slot flow.
- Loading, not found, empty, and error states.

## Non-Goals

- Public business search by text/city/category.
- Featured businesses API.
- Appointment creation.
- Public reviews, images, or media uploads.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/businesses/by-slug/{slug}/profile` | None | Slug route parameter | `200 BusinessProfileResponse` | `404` |
| `GET /api/businesses/{businessId}` | None | Business id | `200 BusinessResponse` | `404` |
| `GET /api/businesses/{businessId}/profile` | None | Business id | `200 BusinessProfileResponse` | `404` |
| `GET /api/businesses/{businessId}/services` | None | Business id | `200 BusinessServiceResponse[]` | `404` |
| `GET /api/businesses/{businessId}/services/{serviceId}` | None | Business and service ids | `200 BusinessServiceResponse` | `404` |
| `GET /api/businesses/{businessId}/staff-members` | None | Business id | `200 BusinessStaffMemberResponse[]` | `404` |
| `GET /api/businesses/{businessId}/staff-members/{staffMemberId}` | None | Business and staff ids | `200 BusinessStaffMemberResponse` | `404` |

The primary route should use `GET /api/businesses/by-slug/{slug}/profile` to avoid multiple calls.

## Design References

- `designs/homepage.op`
- `designs/business-public-profile.op`

## Frontend Routes And Screens

- `/`
- `/b/:slug`

## UX States

- Landing page static/marketing state.
- Loading business profile.
- Not found business profile.
- Empty services or staff members.
- Generic API error.
- CTA to `/b/:slug/appointment`.

## Data And Validation Rules

- Display slots and appointment-related times using business timezone when known.
- Display only backend-provided active public services/staff members.
- Do not infer unavailable services/staff from admin data.

## Implementation Plan

1. Add public business profile TypeScript contracts.
2. Add public business API functions and query hooks.
3. Implement homepage from design.
4. Implement `/b/:slug` profile loading and rendering.
5. Display services, staff members, and business contact/address/timezone data.
6. Add CTA to appointment slot flow.
7. Cover loading, not found, empty, and error states.
8. Add tests for successful profile, not found, and CTA links.
9. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: profile render, empty states, not found/error state, CTA route.
- Integration/manual checks: load a real business by slug against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [x] `/` is not a placeholder and follows `designs/homepage.op`.
- [x] `/b/:slug` loads real backend profile data.
- [x] Active services and staff members are displayed.
- [x] Missing business renders a clear not found state.
- [x] CTA routes to `/b/:slug/appointment`.
- [x] Relevant frontend checks pass.
- [x] `task-groups-roadmap.md` is updated for completed frontend IDs.

## Open Questions

- Resolved: the first MVP homepage uses static content and a demo profile link. Public search and featured business endpoints remain out of scope.
