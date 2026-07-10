#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [[ ! -f .env ]]; then
  echo "Missing backend/.env. Create it with: cp .env.example .env" >&2
  exit 1
fi

docker compose up -d --wait
dotnet run --project src/Calendar.Api --launch-profile http
