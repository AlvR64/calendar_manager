# AGENTS.md

## Repository Scope
- Put backend work under `backend/`; `frontend/` exists but is currently empty.
- Repo-local OpenCode skills live in `.agents/skills` and are wired by `opencode.json`. Use `domain-modeling` for domain terminology changes and `database-schema-designer` for schema/EF modeling work.

## Backend Shape
- The backend solution is `backend/Calendar.slnx` and targets `.NET 10`.
- Projects are layered as `Calendar.Api`, `Calendar.Application`, `Calendar.Domain`, and `Calendar.Infrastructure` under `backend/src/`.
- Keep domain entities in `Calendar.Domain/Entities`; EF Core mapping belongs in `Calendar.Infrastructure/Persistence/Configurations`.
- `Calendar.Api/Program.cs` only wires infrastructure and exposes `GET /health` for now; there are no controllers yet.
- `Calendar.Infrastructure.DependencyInjection.AddInfrastructure` requires `ConnectionStrings:DefaultConnection` and registers `CalendarDbContext` with SQL Server.

## Domain Decisions Already Made
- Use `Appointment`, not `Booking`, for customer reservations.
- Appointments are scheduled against a `StaffMember`; availability is per staff member, not per business.
- `Admin` and `Customer` are separate entities with their own email/password hash fields; do not introduce a generic `User` unless the model is deliberately redesigned.
- `StaffMember` represents the person/resource with an agenda and does not log in in the current MVP.

## Local Infrastructure
- SQL Server is defined in `backend/docker-compose.yml` as container `calendar-sqlserver` on host port `1433`.
- `backend/.env` is intentionally ignored; copy values from `backend/.env.example` if it is missing.
- Development connection string is in `backend/src/Calendar.Api/appsettings.Development.json` and targets `CalendarDb` on `localhost,1433`.

## Commands
- Run commands from `backend/` unless noted.
- Restore local tools first if needed: `dotnet tool restore`.
- Build: `dotnet build`.
- Start SQL Server: `docker compose up -d`.
- Apply migrations: `dotnet tool run dotnet-ef database update --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext`.
- Add a migration after changing EF mappings/entities: `dotnet tool run dotnet-ef migrations add <Name> --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext --output-dir Persistence/Migrations`.
- Run API: `dotnet run --project src/Calendar.Api`.
- Focused health check after running the API: request `GET http://localhost:5167/health` unless launch settings change.

## Verification Notes
- There are no test projects or CI workflows yet; use `dotnet build` plus EF migration/database-update checks for backend validation.
- Use the local EF tool from `backend/dotnet-tools.json` (`dotnet tool run dotnet-ef`), not a globally installed `dotnet ef`, to avoid version mismatches.

## Git Workflow
- When creating a commit, push the branch immediately afterward if a remote is configured; if no remote exists, report that push is blocked by missing remote configuration.
