# Calendar Manager

Local development scripts for running the Calendar Manager MVP.

## Requirements

- Docker with Docker Compose.
- .NET 10 SDK.
- Node.js and npm.
- Bash-compatible shell.

## Initial Setup

Create the backend environment file if it does not exist:

```bash
cp backend/.env.example backend/.env
```

Install frontend dependencies:

```bash
npm install --prefix frontend
```

Apply database migrations after creating a fresh database, pulling new migrations, or resetting SQL Server data:

```bash
bash ./backend/update-db.sh
```

If your local database was created from an older migration baseline, drop it before applying the current initial migration:

```bash
cd backend
dotnet tool run dotnet-ef database drop --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext
bash ./update-db.sh
```

## Start Scripts

Start SQL Server, backend API, and frontend together:

```bash
bash ./start-full-dev.sh
```

The full dev script keeps the terminal attached to backend and frontend logs. This is expected; press `Ctrl+C` to stop both processes.

Start only backend dependencies and API:

```bash
bash ./backend/start-dev.sh
```

Start only frontend:

```bash
bash ./frontend/start-dev.sh
```

## Local URLs

- Frontend: `http://localhost:5173`
- API: `http://localhost:5167`
- API health check: `http://localhost:5167/health`

## MVP Manual Flow

1. Register a business and initial admin from the public UI.
2. Log in as admin.
3. Complete business settings.
4. Create active services.
5. Create active staff members.
6. Assign staff members to services.
7. Configure staff availability.
8. Open `/b/{slug}` to review the public business profile.
9. Open `/b/{slug}/appointment` to select a service, optional staff member, date, and slot.

Appointment creation is intentionally outside the current first MVP slice; the public flow stops after local slot selection.

## Notes

- `backend/start-dev.sh` starts SQL Server through Docker Compose and then runs the API.
- `backend/update-db.sh` is intentionally separate so migrations run only when explicitly requested.
- Development CORS allows the Vite dev server origins `http://localhost:5173` and `http://127.0.0.1:5173`.
