#!/usr/bin/env bash
# Day 1 AM - verifies the licensed Gemini Code Assist environment (macOS / Linux / WSL)
# Usage (repo root):  bash scripts/verify-env.sh [--skip-model-call]
set -u
SKIP_MODEL=0; [ "${1:-}" = "--skip-model-call" ] && SKIP_MODEL=1
FAILS=0; ROWS=""

res() { # check status detail
  local c="$1" s="$2" d="$3"
  ROWS="${ROWS}| ${c} | ${s} | ${d//|//} |"$'\n'
  [ "$s" = "FAIL" ] && FAILS=$((FAILS+1))
  printf '[%s] %s - %s\n' "$s" "$c" "$d"
}
have() { command -v "$1" >/dev/null 2>&1; }
ver() { # check exe regex min
  local c="$1" e="$2" re="$3" min="$4"
  if ! have "$e"; then res "$c" FAIL "$e not found on PATH"; return; fi
  local out; out="$("$e" --version 2>&1 | head -1)"
  local maj; maj="$(echo "$out" | grep -Eo "$re" | head -1 | grep -Eo '[0-9]+' | head -1)"
  if [ -z "$maj" ]; then res "$c" WARN "could not parse: $out"; return; fi
  if [ "$maj" -ge "$min" ]; then res "$c" PASS "$out"; else res "$c" FAIL "$out (need >= $min)"; fi
}

echo "== Book Store SDD - environment verification =="
ver "Git" git '[0-9]+\.[0-9]+\.[0-9]+' 2
ver "Node.js" node 'v[0-9]+' 20
PY=python3; have python3 || PY=python
if have "$PY"; then
  pv="$($PY -c 'import sys;print(f"{sys.version_info[0]}.{sys.version_info[1]}")')"
  if [ "${pv#3.}" -ge 11 ] 2>/dev/null; then res "Python" PASS "$pv"; else res "Python" FAIL "$pv (Spec Kit needs 3.11+)"; fi
else res "Python" FAIL "python not found"; fi
ver "uv" uv '[0-9]+\.[0-9]+' 0
if have dotnet && dotnet --list-sdks | grep -q '^10\.'; then res ".NET SDK 10" PASS "$(dotnet --list-sdks | grep '^10\.' | head -1)"; else res ".NET SDK 10" FAIL "no 10.x SDK"; fi
if have docker && docker info >/dev/null 2>&1; then
  res "Docker daemon" PASS "running"
  docker compose up -d db >/dev/null 2>&1; sleep 3
  r="$(docker compose exec -T db pg_isready 2>&1)"
  echo "$r" | grep -q "accepting" && res "PostgreSQL (compose db)" PASS "$r" || res "PostgreSQL (compose db)" WARN "$r"
else res "Docker daemon" FAIL "not running / not installed"; fi
if have code; then code --list-extensions | grep -qi 'google.geminicodeassist' && res "VS Code + Gemini Code Assist" PASS "installed" || res "VS Code + Gemini Code Assist" FAIL "extension missing"; else res "VS Code CLI" WARN "'code' not on PATH"; fi
ver "Gemini CLI" gemini '[0-9]+\.[0-9]+\.[0-9]+' 0
[ -n "${GOOGLE_CLOUD_PROJECT:-}" ] && res "GOOGLE_CLOUD_PROJECT" PASS "$GOOGLE_CLOUD_PROJECT" || res "GOOGLE_CLOUD_PROJECT" FAIL "not set"
[ -n "${GEMINI_API_KEY:-}" ] && res "GEMINI_API_KEY" WARN "set - unset it so the licensed seat is used"
if [ $SKIP_MODEL -eq 0 ] && have gemini; then
  raw="$(gemini -p 'Reply with exactly the text LICENSED-OK and nothing else.' --output-format json 2>&1)"; code=$?
  if [ $code -eq 0 ] && echo "$raw" | grep -q 'LICENSED-OK'; then res "Licensed model call (headless)" PASS "exit 0"; else res "Licensed model call (headless)" FAIL "exit $code"; fi
fi

mkdir -p docs/lab
host="$(hostname | tr '[:upper:]' '[:lower:]')"
out="docs/lab/env-check-${host}.md"
{
  echo "# Environment verification - ${host}"; echo
  echo "Run at: $(date '+%Y-%m-%d %H:%M')  |  Result: $([ $FAILS -eq 0 ] && echo VERIFIED || echo "$FAILS FAIL(S)")"; echo
  echo "| Check | Status | Detail |"; echo "|---|---|---|"; printf '%s' "$ROWS"; echo
  echo "## Interactive checks (do these by hand, then tick)"; echo
  echo "- [ ] VS Code status bar shows Gemini Code Assist signed in with the CGI account and licensed project"
  echo "- [ ] Chat panel answers; Agent mode toggle visible"
  echo "- [ ] \`gemini\` → \`/about\` shows Code Assist auth / CGI project"
  echo "- [ ] \`/stats model\` shows request counts"
} > "$out"
echo "Wrote $out"
[ $FAILS -eq 0 ] && { echo "Environment VERIFIED."; exit 0; } || { echo "$FAILS check(s) failed."; exit 1; }
