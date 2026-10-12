#!/usr/bin/env bash
# Day 3 PM capstone — full regression (macOS / Linux). Usage: bash scripts/regression.sh [--containers]
set -u
RESULTS=""; FAILS=0
step() { local name="$1"; shift; echo; echo "=== $name ==="; local t0=$SECONDS
  if "$@"; then r=PASS; else r=FAIL; FAILS=$((FAILS+1)); fi
  RESULTS="${RESULTS}| ${name} | ${r} | $((SECONDS-t0)) |"$'\n'; }
web_install() { [ -d web/node_modules ] || npm --prefix web ci; }
step "Database up" docker compose up -d --wait db
step "Constitution hook cases" node .gemini/hooks/test-constitution-guard.mjs
step "API tests (xUnit)" dotnet test --nologo
step "Web install" web_install
step "Web build (strict TS)" npm --prefix web run build
step "UI tests (local stack)" npm --prefix web run test:e2e
if [ "${1:-}" = "--containers" ]; then
  step "Full stack build + up" docker compose --profile full up -d --build --wait
  step "UI tests (containers)" env E2E_BASE_URL=http://localhost:8080 npm --prefix web run test:e2e
fi
mkdir -p docs/lab; f="docs/lab/regression-$(date +%Y%m%d-%H%M).md"
{ echo "# Regression run - $(date '+%Y-%m-%d %H:%M')"; echo; echo "Result: $([ $FAILS -eq 0 ] && echo GREEN || echo "$FAILS FAILED")"; echo
  echo "| Step | Result | Seconds |"; echo "|---|---|---|"; printf '%s' "$RESULTS"; } > "$f"
cat "$f"; [ $FAILS -eq 0 ]
