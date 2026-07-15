#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ ! -f "$SCRIPT_DIR/backend/.env" ]]; then
  echo "Missing backend/.env. Create it with: cp backend/.env.example backend/.env" >&2
  exit 1
fi

cleanup() {
  if [[ -n "${BACKEND_PID:-}" ]]; then
    kill "$BACKEND_PID" 2>/dev/null || true
  fi

  if [[ -n "${FRONTEND_PID:-}" ]]; then
    kill "$FRONTEND_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

(
  cd "$SCRIPT_DIR/backend"
  docker compose up -d --wait
  dotnet run --project src/Calendar.Api --launch-profile http
) &
BACKEND_PID=$!

(
  cd "$SCRIPT_DIR/frontend"
  npm run dev
) &
FRONTEND_PID=$!

wait -n "$BACKEND_PID" "$FRONTEND_PID"
