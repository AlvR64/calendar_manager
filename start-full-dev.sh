#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

log() {
  printf '\n[%s] %s\n' "$(date +%H:%M:%S)" "$*"
}

if [[ ! -f "$SCRIPT_DIR/backend/.env" ]]; then
  echo "Missing backend/.env. Create it with: cp backend/.env.example backend/.env" >&2
  exit 1
fi

cleanup() {
  if [[ "${CLEANED_UP:-0}" == "1" ]]; then
    return
  fi

  CLEANED_UP=1
  log "Stopping development processes..."

  if [[ -n "${BACKEND_PID:-}" ]]; then
    kill "$BACKEND_PID" 2>/dev/null || true
    wait "$BACKEND_PID" 2>/dev/null || true
  fi

  if [[ -n "${FRONTEND_PID:-}" ]]; then
    kill "$FRONTEND_PID" 2>/dev/null || true
    wait "$FRONTEND_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

log "Starting SQL Server..."
(
  cd "$SCRIPT_DIR/backend"
  docker compose up -d --wait
)
log "SQL Server is ready."

log "Starting backend API at http://localhost:5167"
(
  cd "$SCRIPT_DIR/backend"
  dotnet run --project src/Calendar.Api --launch-profile http
) &
BACKEND_PID=$!

log "Starting frontend at http://localhost:5173"
(
  cd "$SCRIPT_DIR/frontend"
  npm run dev
) &
FRONTEND_PID=$!

cat <<'EOF'

Development servers are running:
- Frontend: http://localhost:5173
- API: http://localhost:5167
- API health check: http://localhost:5167/health

This terminal stays attached to the dev servers. Press Ctrl+C to stop both.
EOF

set +e
wait -n "$BACKEND_PID" "$FRONTEND_PID"
EXIT_CODE=$?
set -e

if ! kill -0 "$BACKEND_PID" 2>/dev/null; then
  log "Backend API stopped; shutting down frontend."
elif ! kill -0 "$FRONTEND_PID" 2>/dev/null; then
  log "Frontend stopped; shutting down backend API."
else
  log "A development process stopped; shutting down the rest."
fi

exit "$EXIT_CODE"
