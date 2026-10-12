<!--
Sync Impact Report
- Version change: template → 1.0.0 (initial ratification)
- Principles added: I Specification Before Code, II Contract-First APIs, III Layered Backend,
  IV Money Is Decimal, V Test-Gated Delivery, VI Secrets Stay Out, VII Simplicity and Restraint
- Sections added: Technology Constraints, Development Workflow and Quality Gates, Governance
- Templates requiring updates:
  ✅ .specify/templates/plan-template.md (Constitution Check reads gates from this file — no edit needed)
  ✅ .specify/templates/spec-template.md (no conflicting sections)
  ✅ .specify/templates/tasks-template.md (test tasks are now mandatory per Principle V — enforced at /speckit.tasks time)
- Deferred TODOs: none
-->

# Online Book Store Constitution

## Core Principles

### I. Specification Before Code

Every feature MUST have a `spec.md`, `plan.md` and `tasks.md` under `specs/<###-feature>/` before any
production code for it is written. Every code change MUST trace to a task ID in a `tasks.md`.
A spec MUST contain an explicit **Out of Scope** section. Ambiguities MUST be resolved with
`/speckit.clarify` and recorded in the spec's Clarifications section, not guessed during implementation.

**Rationale:** the model fills every gap you leave. Written intent is the only gap-filler you control.

### II. Contract-First APIs

Every HTTP endpoint MUST be described in an OpenAPI 3.1 document in `specs/<###-feature>/contracts/` before
it is implemented, and the implementation MUST match it. Routes MUST be versioned under `/api/v1/`.
Responses MUST be DTOs, never EF Core entities. Errors MUST be RFC 9457 Problem Details.
Breaking a published contract MUST require a new version prefix.

**Rationale:** the contract is the seam between parallel workers — human or agent.

### III. Layered Backend

Controllers MUST only translate HTTP to service calls and MUST NOT reference the DbContext.
Business rules MUST live in services behind interfaces. Data access MUST use EF Core LINQ;
`FromSqlRaw`/`ExecuteSqlRaw` with interpolated or concatenated strings MUST NOT be used.
New code MUST live under `Features/<Feature>/` or `Infrastructure/`; legacy folders MUST NOT grow.

**Rationale:** agents copy the nearest pattern. Layers keep the nearest pattern a good one.

### IV. Money Is Decimal (NON-NEGOTIABLE)

Monetary values MUST be `decimal` in C# and `numeric(10,2)` in PostgreSQL. `double` and `float` MUST NOT
be used for any price, amount, subtotal, discount or total. Monetary calculations MUST happen on the
server; the client MUST only format values it receives.

**Rationale:** floating-point money is the defect that passes every demo and fails every audit.

### V. Test-Gated Delivery (NON-NEGOTIABLE)

Every user story MUST have at least one xUnit API test derived from its acceptance scenarios.
Every user-facing journey MUST have at least one Playwright test. A task MUST NOT be marked complete
while `dotnet test` or the Playwright suite is failing. Tests MUST NOT be deleted, skipped or weakened
to obtain a green run.

**Rationale:** an agent's "done" is a claim. A green suite is evidence.

### VI. Secrets Stay Out

Credentials MUST NOT appear in source code, committed configuration, prompts, logs or commit messages.
Configuration MUST come from environment variables or `dotnet user-secrets`. `.aiexclude` and
`.geminiignore` MUST cover every secret path. The single permitted exception is the local-only Docker
development password in `appsettings.Development.json`.

**Rationale:** whatever the agent can read, it can repeat.

### VII. Simplicity and Restraint

The solution MUST remain one API project, one test project and one web app. A new NuGet or npm
dependency MUST be justified in the feature's `research.md` before it is added. Features MUST NOT
include speculative capabilities that no user story requires (YAGNI).

**Rationale:** every dependency and abstraction an agent adds is one a human must maintain.

## Technology Constraints

- Runtime: .NET 10 (ASP.NET Core Web API, controllers), EF Core 10 with Npgsql, PostgreSQL 16.
- Frontend: React 19, Vite, TypeScript in strict mode, React Router.
- Tests: xUnit with `WebApplicationFactory`; Playwright (Chromium) for UI.
- Local execution only: Docker Desktop for PostgreSQL and, from Day 3, for the full stack.
- Tooling: Gemini Code Assist (VS Code agent mode) and Gemini CLI on licensed Standard/Enterprise seats.

## Development Workflow and Quality Gates

1. `/speckit.specify` → `/speckit.clarify` → Plan Mode review → `/speckit.plan` → `/speckit.tasks`
   → `/speckit.analyze` → `/speckit.implement` → `/speckit.converge`.
2. The plan's **Constitution Check** MUST pass before research and again after design. Any violation MUST
   be justified in the plan's Complexity Tracking table or the plan MUST be changed.
3. Checkpointing MUST be enabled for any agent session that edits files.
4. Every merge to `main` MUST go through a pull request reviewed by Gemini Code Assist and approved by a
   human. The human reviewer carries the licence and attribution check, because agent mode does not show
   source citations.
5. CI MUST run `dotnet test`, the frontend build and the headless constitution gate; a non-zero exit blocks merge.

## Governance

This constitution supersedes every other practice, prompt and context file in this repository.
Amendments MUST be made by pull request that updates this file, bumps the version (MAJOR: principle
removed or redefined; MINOR: principle or section added; PATCH: wording) and updates the Sync Impact
Report. Every pull request review MUST verify compliance. Runtime guidance for the agent lives in
`GEMINI.md`, which MUST NOT contradict this document.

**Version**: 1.0.0 | **Ratified**: 2026-10-12 | **Last Amended**: 2026-10-12
