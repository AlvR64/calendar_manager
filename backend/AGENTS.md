# Backend AGENTS.md

## Backend Scope
- Applies to files under `backend/`.
- The backend solution is `Calendar.slnx` and targets `.NET 10`.
- Projects are layered as `Calendar.Api`, `Calendar.Application`, `Calendar.Domain`, and `Calendar.Infrastructure` under `src/`.
- Use `aspnet-core` for ASP.NET Core host/DI/middleware/auth/configuration guidance.
- Use `dotnet-webapi` for Web API endpoints, HTTP semantics, OpenAPI, and error handling.
- Use `database-schema-designer` for schema/EF modeling work.
- Use `csharp-xunit` when adding or refactoring xUnit tests.

## Backend Roadmap
- `../task-groups-roadmap.md` is the single source of truth for backend, frontend, and design task status.
- Use its group/use-case IDs when backend task IDs are referenced.
- Keep the backend column synchronized as backend work is completed.

## Architecture
- Keep domain entities in `Calendar.Domain/Entities`.
- EF Core mapping belongs in `Calendar.Infrastructure/Persistence/Configurations`.
- `Calendar.Api/Program.cs` wires application/infrastructure services, maps controllers, and exposes `GET /health`.
- `Calendar.Infrastructure.DependencyInjection.AddInfrastructure` requires `ConnectionStrings:DefaultConnection` and registers `CalendarDbContext` with SQL Server.
- Prefer controller-based Web API endpoints; do not switch to minimal APIs unless explicitly requested.
- Follow the existing CQRS style with `ICommandHandler` and `IQueryHandler`; do not introduce MediatR unless explicitly requested.

## Local Infrastructure
- SQL Server is defined in `docker-compose.yml` as container `calendar-sqlserver` on host port `1433`.
- `.env` is intentionally ignored; copy values from `.env.example` if it is missing.
- Development connection string is in `src/Calendar.Api/appsettings.Development.json` and targets `CalendarDb` on `localhost,1433`.

## Commands
- Run commands from `backend/` unless noted.
- Restore local tools first if needed: `dotnet tool restore`.
- Build: `dotnet build`.
- Test: `dotnet test`.
- Start SQL Server: `docker compose up -d`.
- Apply migrations: `dotnet tool run dotnet-ef database update --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext`.
- Add a migration after changing EF mappings/entities: `dotnet tool run dotnet-ef migrations add <Name> --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext --output-dir Persistence/Migrations`.
- Run API: `dotnet run --project src/Calendar.Api`.
- Focused health check after running the API: request `GET http://localhost:5167/health` unless launch settings change.

## Verification Notes
- After backend code changes, add or update relevant tests in the appropriate test project.
- Use FluentAssertions for test assertions in .NET test projects.
- Use `dotnet test` plus `dotnet build` for backend validation.
- Add EF migration/database-update checks when EF mappings/entities change.
- Use the local EF tool from `dotnet-tools.json` (`dotnet tool run dotnet-ef`), not a globally installed `dotnet ef`, to avoid version mismatches.
