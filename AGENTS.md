# AGENTS.md

## Repository Scope
- The GitHub repo is named `calendar_manager`. If the local folder still appears as `calendar_manager_dotnet`, that is only a local directory name; Git remote/tracking is already updated.
- Backend work lives under `backend/`; backend-specific instructions live in `backend/AGENTS.md`.
- Frontend work lives under `frontend/`; frontend-specific instructions live in `frontend/AGENTS.md`.
- Repo-local OpenCode skills live in `.agents/skills` and are wired by `opencode.json`.
- Use `domain-modeling` for domain terminology changes, `find-skills` to search for additional repo-local skills, and area-specific skills from the nearest scoped `AGENTS.md`.

## Product Context
- Calendar manager is a SaaS product for businesses that expose public services, staff members, availability, and appointments.
- Businesses are managed by admins.
- Customers reserve appointments for services with available staff members.
- Staff members represent people/resources with agendas in the current MVP.

## Unified Roadmap
- `task-groups-roadmap.md` is the single source of truth for backend, frontend, and design task status.
- Use its group/use-case IDs when task IDs are referenced.
- Keep backend, frontend, and design columns synchronized as work is completed.

## Domain Language
- Use `Appointment`, not `Booking`, for customer reservations.
- Appointments are scheduled against a `StaffMember`.
- Availability is per `StaffMember`, not per business.
- `Admin` and `Customer` are separate account types; do not introduce a generic `User` unless the model is deliberately redesigned.
- `StaffMember` does not log in in the current MVP.
- Keep backend, frontend, docs, tests, API contracts, and UI copy aligned with this terminology.

## Cross-Cutting Decisions
- Business timezone is represented as an IANA timezone id, for example `Europe/Madrid`.
- Appointment instants are UTC-first in backend APIs.
- User-facing calendar, appointment, and slot displays should respect the business timezone when business context is known.
- Available slot responses may include both local and UTC times; prefer backend-provided slot data over recomputing availability in clients.

## Git Workflow
- Remote `origin` is `https://github.com/AlvR64/calendar_manager.git`; default branch is `main`.
- GitHub CLI is authenticated as `AlvR64`; if `gh` is not on PATH in a fresh shell, use `C:\Program Files\GitHub CLI\gh.exe`.
- After making changes, create a commit for those changes; every commit must be pushed immediately afterward.
- When creating a commit, push the branch immediately afterward if a remote is configured; if no remote exists, report that push is blocked by missing remote configuration.
- Do not create a PR unless explicitly requested.
