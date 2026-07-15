#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project src/Calendar.Infrastructure \
  --startup-project src/Calendar.Api \
  --context CalendarDbContext
