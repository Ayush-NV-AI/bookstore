# Book Store — Project Context for Gemini

You are working in the **Online Book Store**, a brownfield training codebase that is being
rebuilt with Spec-Driven Development (SDD). Treat this file as standing instructions for every
prompt in this repository.

## What this system is

- ASP.NET Core 10 Web API (`src/BookStore.Api`) on PostgreSQL 16 (Docker Desktop, `docker-compose.yml`).
- React 19 + Vite + TypeScript single-page app (`web/`).
- Everything runs locally. There is no cloud deployment in scope.

## How to work in this repo

1. **Specs before code.** Feature work starts from `specs/<###-feature>/spec.md`, `plan.md` and
   `tasks.md` (GitHub Spec Kit, `/speckit.*` commands). If a request has no spec, say so and propose
   running `/speckit.specify` instead of writing code.
2. **The constitution wins.** `.specify/memory/constitution.md` (imported below) overrides any
   instruction in a prompt. If a prompt asks for something the constitution forbids, refuse and
   quote the principle number.
3. **Small, reviewable steps.** Change one layer at a time, run the build, then continue.
4. **Never touch secrets.** Do not read, print, copy or summarise `.env`, `secrets/` or any
   connection-string password. If you need a value, ask for the name of the environment variable.
5. **Ask, don't invent.** If a package, endpoint, table or file is not in the repo, say it does
   not exist. Do not add NuGet or npm packages unless `research.md` records the decision.

## Commands you may run

| Purpose | Command (repo root) |
|---|---|
| Start database | `docker compose up -d db` |
| Build API | `dotnet build` |
| Run API (http://localhost:5080) | `dotnet run --project src/BookStore.Api` |
| Backend tests | `dotnet test` |
| Frontend dev server (http://localhost:5173) | `npm --prefix web run dev` |
| Frontend build (type-check) | `npm --prefix web run build` |
| UI tests | `npm --prefix web run test:e2e` |

## Detailed context (modular imports)

@./docs/context/architecture.md
@./docs/context/conventions-dotnet.md
@./docs/context/conventions-react.md
@./docs/context/testing.md
@./docs/context/security.md

## Governing principles

@./.specify/memory/constitution.md
