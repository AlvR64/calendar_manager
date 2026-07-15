#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

log() {
  printf '\n[%s] %s\n' "$(date +%H:%M:%S)" "$*"
}

if [[ ! -f .env ]]; then
  echo "Missing backend/.env. Create it with: cp .env.example .env" >&2
  exit 1
fi

log "Starting SQL Server..."
docker compose up -d --wait
log "SQL Server is ready."

cat <<'EOF'

Backend API is starting:
- API: http://localhost:5167
- API health check: http://localhost:5167/health

This terminal stays attached to the API. Press Ctrl+C to stop it.
EOF

dotnet run --project src/Calendar.Api --launch-profile http
