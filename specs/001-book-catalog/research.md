# Research: Book Catalog

Every decision below follows the format **Decision / Rationale / Alternatives considered**.
New dependencies are justified here per Constitution VII.

## R1 — Price type

- **Decision**: `decimal` in C#, `numeric(10,2)` in PostgreSQL (`HasPrecision(10, 2)`).
- **Rationale**: Constitution IV. `double` produces representation errors (`0.1 + 0.2`).
- **Alternatives**: integer paise (rejected — every consumer must remember to divide; harder to read in SQL).

## R2 — Legacy endpoints and discount logic

- **Decision**: delete the legacy controller, helper and client discount/tax maths (FR-012).
- **Rationale**: Clarification Q3; wrapping legacy code would keep the injection-prone search alive.
- **Alternatives**: keep legacy routes as a façade over the new service (rejected — no remaining consumer after the React rewrite).

## R3 — Case-insensitive search

- **Decision**: `b.Title.ToLower().Contains(term) || b.Author.ToLower().Contains(term)` with `term` lower-cased once.
- **Rationale**: translated by Npgsql to a parameterised `lower(...) LIKE '%' || @p || '%'`; also works on the EF InMemory provider used by API tests.
- **Alternatives**: `EF.Functions.ILike` (Npgsql-only, breaks InMemory tests); PostgreSQL full-text search (out of scope); `pg_trgm` index (deferred until SC-002 is measured).

## R4 — Pagination shape

- **Decision**: offset pagination (`page`, `pageSize`) returning `PagedResult<T> { items, page, pageSize, totalCount, totalPages }`.
- **Rationale**: the UI needs "Page X of Y" and a total count; catalogue size is small.
- **Alternatives**: keyset/cursor pagination (rejected — no jump-to-page, more complex for a 30–1,000 row table).

## R5 — Validation and errors

- **Decision**: validate query parameters in the controller with `ModelState` + `ValidationProblem()`; built-in `AddProblemDetails()` for 404/500.
- **Rationale**: Constitution II requires RFC 9457 Problem Details; ASP.NET Core produces them natively.
- **Alternatives**: FluentValidation (rejected — new dependency for four rules).

## R6 — Frontend routing (new dependency)

- **Decision**: `react-router-dom` 7.
- **Rationale**: detail page + URL-driven filters (FR-010) need a router; it is the de-facto standard.
- **Alternatives**: hand-rolled `window.location` parsing (rejected — reinvents history handling and back-button behaviour).

## R7 — UI test framework (new dependency)

- **Decision**: `@playwright/test`, Chromium only, `webServer` starts API + Vite.
- **Rationale**: Constitution V mandates a UI journey test; Playwright auto-waits and has trace viewer for debugging agent-written tests.
- **Alternatives**: Cypress (rejected — heavier, second browser runtime); component tests only (rejected — do not prove API + DB integration).

## R8 — API test stack (new dependencies)

- **Decision**: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`.
- **Rationale**: in-process API tests are fast (< 5 s) and need no Docker; Playwright covers real PostgreSQL.
- **Alternatives**: Testcontainers PostgreSQL (better fidelity, but adds Docker start-up per run and fails on locked-down laptops; reconsider for CI).

## R9 — Schema management

- **Decision**: keep `EnsureCreated()`; document `docker compose down -v` when the schema changes.
- **Rationale**: training codebase; migrations are explicitly out of scope in spec.md.
- **Alternatives**: EF Core migrations (correct for production — tracked as follow-up).
