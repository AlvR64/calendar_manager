#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

cat <<'EOF'

Frontend dev server is starting:
- Frontend: http://localhost:5173

This terminal stays attached to Vite. Press Ctrl+C to stop it.
EOF

npm run dev
