# Frontend AGENTS.md

## Frontend Scope
- Applies to files under `frontend/`.
- The frontend is a React SPA built with Vite and TypeScript.
- Use React Router for routing.
- Use TanStack Query for server state, API loading/error handling, caching, refetching, and mutation invalidation.
- Use Tailwind CSS for styling.
- Use shadcn/ui and Radix UI primitives for accessible UI components when useful.
- Use React Hook Form and Zod for forms and client-side validation.
- Use Vitest and Testing Library for frontend unit/component tests once tests are added.
- Use Playwright for end-to-end flows if/when E2E coverage is introduced.

## Frontend Roadmap
- `../task-groups-roadmap.md` is the single source of truth for backend, frontend, and design task status.
- Use its group/use-case IDs when frontend task IDs are referenced.
- Keep the frontend column synchronized as frontend work is completed.

## Frontend Task Specs
- Product/use-case specs live under `../docs/task-specs/`.
- First MVP frontend specs live under `../docs/task-specs/first-mvp/`.
- Before implementing a first MVP frontend slice, read the matching spec when it exists.
- Create missing specs from `../docs/task-specs/TEMPLATE.md` when a slice needs clearer scope before implementation.
- `../task-groups-roadmap.md` remains the source of truth for frontend status.
- Mark the roadmap `Frontend` column only when the UI/API behavior is implemented and verified, not for placeholders.

## Product And Domain Language
- Follow the root `AGENTS.md` domain language.
- Use `Appointment`, not `Booking`.
- Use `StaffMember` in code/domain concepts; UI copy may display `staff member` or `staff` where shorter text is needed.
- `Admin` and `Customer` are separate auth/account flows.
- Do not introduce a generic `User` model unless the domain model is deliberately redesigned.
- Staff members do not log in in the current MVP.
- Availability is per `StaffMember`.
- Business timezone is an IANA timezone id, for example `Europe/Madrid`.

## Frontend Architecture
- Keep the frontend as a client-side SPA for the MVP.
- Public routes and admin routes should live in the same Vite app unless the architecture is deliberately changed.
- Prefer feature-oriented folders for non-trivial areas:
  - `features/auth`
  - `features/businesses`
  - `features/services`
  - `features/staffMembers`
  - `features/availability`
  - `features/appointmentSlots`
- Keep reusable layout components under `layouts/`.
- Keep generic UI primitives under `components/ui/`.
- Keep API clients and DTO/type definitions under `api/`.
- Keep route constants and cross-cutting helpers under `lib/`.

## Routing
- Use React Router.
- Public MVP routes should include:
  - `/`
  - `/b/:slug`
  - `/b/:slug/appointment`
  - `/auth/customer/login`
  - `/auth/customer/register`
  - `/auth/admin/login`
  - `/auth/business/register`
- Admin MVP routes should include:
  - `/admin`
  - `/admin/business-settings`
  - `/admin/services`
  - `/admin/staff-members`
  - `/admin/availability`
- Admin routes require admin authentication.
- Customer-only routes require customer authentication when added.
- Keep customer and admin auth flows separate.

## API Integration
- The ASP.NET Core backend is the source of truth for API behavior.
- Prefer backend-provided slot data over recomputing availability in the frontend.
- Available slot displays should respect the business timezone when business context is known.
- Appointment instants from backend APIs are UTC-first.
- Keep frontend request/response types aligned with backend contracts.
- For MVP, handwritten TypeScript API types are acceptable.
- Consider generated TypeScript clients from OpenAPI later if API surface grows.

## State Management
- Use TanStack Query for server state.
- Avoid duplicating API state in global stores.
- Use local component state for UI-only state such as selected tab, open modal, selected service, selected staff member, selected date, or selected slot.
- Add a global auth store only for authentication/session state if needed.
- Prefer simple state first; do not add Redux unless there is a concrete need.

## Forms
- Use React Hook Form with Zod schemas for forms.
- Mirror backend validation constraints where practical:
  - required fields
  - email format
  - password minimum length
  - max lengths
  - numeric ranges
  - slug format
  - currency code length
  - IANA timezone input guidance
- Backend validation remains authoritative.

## Styling And UI
- Preserve the visual language from the OpenPencil designs in `designs/`.
- Use Tailwind CSS utilities for layout and styling.
- Use shadcn/ui/Radix for common accessible components:
  - dialogs/modals
  - sheets
  - dropdowns
  - selects
  - buttons
  - inputs
  - tabs
  - toasts
- Keep admin layout consistent across:
  - dashboard
  - business settings
  - services
  - staff members
  - availability
- Public pages should keep the clean marketplace visual style already designed.

## Design Source
- OpenPencil designs live under `designs/`.
- Use the relevant `.op` design as the visual reference before implementing a page.
- MVP design files currently include:
  - `homepage.op`
  - `customer-login.op`
  - `business-admin-login.op`
  - `business-admin-register.op`
  - `business-admin-dashboard.op`
  - `business-settings.op`
  - `admin-services.op`
  - `admin-staff-members.op`
  - `admin-availability.op`
  - `business-public-profile.op`
  - `public-appointment-slot-flow.op`
  - `customer-register.op`
- If behavior cannot be represented interactively in OpenPencil, implement it normally in React while preserving the designed state.

## Commands
- Run frontend commands from `frontend/`.
- Install dependencies: `npm install`.
- Start dev server: `npm run dev`.
- Build: `npm run build`.
- Type check: `npm run typecheck` if configured.
- Lint: `npm run lint` if configured.
- Test: `npm test` or `npm run test` if configured.
- Preview production build: `npm run preview`.

## Verification Notes
- After frontend code changes, run the relevant available checks.
- Prefer at least `npm run build` before committing frontend implementation changes.
- Run lint/typecheck/test commands when they exist.
- For UI work, verify both desktop and mobile layouts.
- For API-connected pages, verify loading, empty, error, and success states when feasible.
- Do not mark a frontend task complete until the route renders and the relevant checks pass.

## Deployment Notes
- MVP frontend target is a Vite React SPA.
- SPA deployment requires fallback to `index.html` for client-side routes.
- If served from ASP.NET Core later, `/api/*` should route to backend controllers and other routes should fall back to the SPA.
- If deployed separately, configure API base URL through environment variables.
