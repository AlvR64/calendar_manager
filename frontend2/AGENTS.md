# Frontend 2 AGENTS.md

## Scope

- Applies to files under `frontend2/`.
- `frontend2/` is an isolated React SPA redesign candidate; `../frontend/` remains the current frontend.
- Do not import source code from `../frontend/` or create dependencies between the two applications.
- Reuse backend contracts and non-visual behavior by maintaining independent copies when needed.

## Stack And Commands

- Use React, TypeScript, Vite, React Router, and TanStack Query.
- Run commands from `frontend2/`: `npm install`, `npm run dev`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, and `npm run build`.
- The Vite development server must run on `http://localhost:5174`.

## Product Rules

- Follow `../AGENTS.md` domain language.
- Use `Appointment`, not `Booking`. Appointments are scheduled against a `StaffMember`.
- Keep Admin and Customer authentication flows separate. Staff members do not log in.
- Respect the business IANA timezone and use backend-provided slot data.

## Architecture And UX

- Keep public and admin routes in this SPA and use protected routes for authenticated areas.
- Keep API code under `src/api/`, auth under `src/auth/`, layouts under `src/layouts/`, and feature code under `src/features/`.
- Build a new visual system. Do not copy layouts, presentation components, page markup, or styles from `../frontend/`.
- Every implemented screen must handle applicable loading, empty, error, validation, and success states and work on desktop and mobile.
- Prefer accessible native semantics and accessible primitives when components are introduced.

## Design Direction

- The master visual guide is `../designs/frontend2/README.md`.
- Create or update frontend2 OpenPencil designs only under `../designs/frontend2/`; designs directly under `../designs/` are existing frontend references and must remain unchanged.
- During a design-only request, use OpenPencil MCP and do not implement React, change backend behavior, or alter business rules unless explicitly requested.

## Specs And Status

- Redesign specs live in `../docs/task-specs-frontend2/`; use its `TEMPLATE.md` for new slices.
- Canonical product behavior and backend contracts remain in `../docs/task-specs/`.
- `../task-groups-roadmap.md` remains authoritative for product status. Do not mark frontend work complete for scaffold or placeholders.
