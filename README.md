# Online Book Store — Spec-Driven Development training codebase

ASP.NET Core 10 Web API · React 19 + Vite + TypeScript · PostgreSQL 16 (Docker Desktop) · Gemini Code Assist.

## Run it

```powershell
docker compose up -d db                      # PostgreSQL on localhost:5432
dotnet run --project src/BookStore.Api       # API on http://localhost:5080
npm --prefix web install
npm --prefix web run dev                     # Web on http://localhost:5173
```

Schema changed since your last run? `docker compose down -v` then start again (no migrations — see research R9).

## Test it

```powershell
dotnet test                                   # xUnit API tests (InMemory provider)
npx --prefix web playwright install chromium  # first time only
npm --prefix web run test:e2e                 # Playwright against the real API + PostgreSQL
```

## How this repo is governed

- `.specify/memory/constitution.md` — the rules. Wins over everything else.
- `GEMINI.md` + `docs/context/*.md` — what the agent is told on every prompt.
- `specs/<###-feature>/` — spec → plan → tasks for every feature.
- `.aiexclude` / `.geminiignore` — what the agent may never read.

## Capstone (Day 3 PM)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\regression.ps1 -Containers   # every suite, both stacks
node scripts/ci/constitution-gate.mjs                                           # headless gate: 0 pass · 1 fail · 2 error
```

Demo flow: `docs/demo/capstone-demo.md`. CI: `.github/workflows/ci.yml`.
