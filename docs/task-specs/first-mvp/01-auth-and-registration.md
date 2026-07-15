# Auth And Registration

## Roadmap IDs

- `1.1`
- `7.1`
- `7.2`
- `8.1`

## Current Status Matrix

Mirror of `task-groups-roadmap.md` at spec creation time. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `1.1` | [x] | [ ] | [x] | [ ] |
| `7.1` | [x] | [ ] | [x] | [ ] |
| `7.2` | [x] | [ ] | [x] | [ ] |
| `8.1` | [x] | [ ] | [x] | [ ] |

## Goal

Implement the first MVP authentication and registration flows so admins and customers can create accounts or sign in against the backend, with separate account/session handling.

## Scope

- Business registration with initial admin account.
- Customer registration.
- Admin login.
- Customer login.
- MVP session persistence using existing frontend auth storage.
- Admin route protection and redirects after successful admin login.
- Loading, validation, success, and backend error states for each form.

## Non-Goals

- Email verification.
- Password reset.
- Refresh tokens or token revocation.
- Customer private dashboard.
- Auto-login after registration unless backend starts returning access tokens.

## Backend Contract

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `POST /api/auth/admin/login` | None | `LoginAdminRequest` | `200 LoginAdminResponse` with token and admin business id | `400`, `401`, `403` |
| `POST /api/auth/customer/login` | None | `LoginCustomerRequest` | `200 LoginCustomerResponse` with token and customer profile summary | `400`, `401`, `403` |
| `POST /api/auth/register-business` | None | `RegisterBusinessRequest` | `201 RegisterBusinessResponse` | `400`, `409` slug/email conflict |
| `POST /api/auth/register-customer` | None | `RegisterCustomerRequest` | `201 RegisterCustomerResponse` | `400`, `409` email conflict |

Registration endpoints do not currently return access tokens. After registration, route users to the matching login page unless backend behavior changes.

## Design References

- `designs/business-admin-login.op`
- `designs/business-admin-register.op`
- `designs/customer-login.op`
- `designs/customer-register.op`

## Frontend Routes And Screens

- `/auth/admin/login`
- `/auth/business/register`
- `/auth/customer/login`
- `/auth/customer/register`
- `/admin` protected route behavior after admin login

## UX States

- Loading while submitting each form.
- Validation errors before submit.
- Backend validation/conflict/auth errors after submit.
- Success redirect after login.
- Success message and login redirect after registration.
- Unauthorized admin route redirects to `/auth/admin/login`.

## Data And Validation Rules

- Admin/customer email must be valid email format.
- Password minimum length is 8.
- Business slug follows `^[a-z0-9]+(?:-[a-z0-9]+)*$`.
- Business time zone must be submitted as IANA id.
- Currency code is exactly 3 letters.
- Backend validation remains authoritative.

## Implementation Plan

1. Add missing auth request/response TypeScript contracts.
2. Add auth API functions for login/register endpoints.
3. Add React Hook Form + Zod schemas for the four forms.
4. Implement admin login and store admin session on success.
5. Implement customer login and store customer session separately from admin.
6. Implement business registration and route to admin login on success.
7. Implement customer registration and route to customer login on success.
8. Wire loading, validation, conflict, unauthorized, and success states.
9. Add component tests for successful submit and common validation/error paths.
10. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests: form validation, submit disabled/loading states, success redirects, backend error rendering.
- Integration/manual checks: login/register flows against local API.
- Commands to run: `npm run typecheck`, `npm run lint`, `npm run build`, `npm test -- --run`.

## Acceptance Criteria

- [ ] All four auth/register screens are real forms, not placeholders.
- [ ] Forms submit to real backend endpoints.
- [ ] Admin login stores an admin session and allows `/admin` access.
- [ ] Customer login stores a customer session without granting admin access.
- [ ] Registration success routes to the matching login flow.
- [ ] Backend validation, auth, and conflict errors are shown clearly.
- [ ] UI follows referenced OpenPencil designs.
- [ ] Relevant frontend checks pass.
- [ ] `task-groups-roadmap.md` is updated for completed frontend IDs.

## Open Questions

- None.
