# Implementation Plan: Book Catalog

**Branch**: `001-book-catalog` | **Date**: 2026-10-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-book-catalog/spec.md`, approach approved in [plan-mode-review.md](./plan-mode-review.md)

## Summary

Replace the vibe-coded book listing with a versioned, contract-first catalog slice: a `Catalog` feature folder
in the API (thin controller → service → EF Core LINQ on PostgreSQL, DTO responses, Problem Details errors), a
typed React catalog with URL-driven search/filter/sort/pagination and a detail page, xUnit API tests and a
Playwright UI suite. Legacy endpoints, the `double` price and client-side discount/tax logic are removed.

## Technical Context

**Language/Version**: C# 14 / .NET 10; TypeScript 5.9

**Primary Dependencies**: ASP.NET Core 10 (controllers), EF Core 10 + Npgsql.EntityFrameworkCore.PostgreSQL 10; React 19, React Router 7, Vite 7

**Storage**: PostgreSQL 16 (Docker Desktop), schema via `EnsureCreated()` (training simplification)

**Testing**: xUnit + `Microsoft.AspNetCore.Mvc.Testing` + EF Core InMemory (API); Playwright Chromium (UI, real DB)

**Target Platform**: Developer laptop (Windows/macOS) — local only

**Project Type**: Web application (API + SPA)

**Performance Goals**: catalog list p95 < 300 ms for 1,000 books (SC-002)

**Constraints**: no new dependency without research.md entry; money is `decimal`; no raw SQL

**Scale/Scope**: 30 seed books, 8 genres; 3 endpoints + health; 2 pages

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-research | Post-design |
|---|---|---|---|
| I Specification Before Code | spec.md has Out of Scope + clarifications; tasks.md will trace every change | PASS | PASS |
| II Contract-First APIs | `contracts/catalog.openapi.yaml` written before code; `/api/v1/catalog`; DTOs; Problem Details | PASS | PASS |
| III Layered Backend | `CatalogController` → `ICatalogService` → `BookStoreDbContext`; LINQ only; legacy folders deleted | PASS | PASS |
| IV Money Is Decimal | `Book.Price` becomes `decimal` / `numeric(10,2)`; discount logic removed; client formats only | PASS | PASS |
| V Test-Gated Delivery | xUnit test per story + Playwright journey per story (see tasks) | PASS | PASS |
| VI Secrets Stay Out | payment key + prod-looking password removed from `appsettings.json` and `Helper.cs`; dev password only in `appsettings.Development.json` | PASS | PASS |
| VII Simplicity | No repository/UoW layer, no Swagger package; new deps: React Router, Playwright, xUnit stack — each justified in research.md | PASS | PASS |

**Result: PASS — no Complexity Tracking entries required.**

## Project Structure

### Documentation (this feature)

```text
specs/001-book-catalog/
├── spec.md
├── plan-mode-review.md   # Plan Mode output, annotated by the architect
├── plan.md               # This file
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── catalog.openapi.yaml
├── checklists/
│   └── requirements.md
└── tasks.md              # /speckit.tasks output
```

### Source Code (repository root)

```text
src/BookStore.Api/
├── Program.cs                         # composition root: DbContext, services, ProblemDetails, CORS, health
├── appsettings.json                   # no secrets
├── appsettings.Development.json       # local Docker connection string only
├── Infrastructure/
│   ├── BookStoreDbContext.cs
│   ├── DbSeeder.cs
│   └── SeedBooks.cs
└── Features/Catalog/
    ├── Book.cs
    ├── CatalogContracts.cs            # BookSummary, BookDetail, PagedResult<T>, CatalogQuery, CatalogSort
    ├── CatalogService.cs              # ICatalogService + CatalogService
    └── CatalogController.cs

tests/BookStore.Api.Tests/
├── BookStore.Api.Tests.csproj
├── BookStoreApiFactory.cs
└── Catalog/CatalogApiTests.cs

web/
├── vite.config.ts                     # /api proxy → http://localhost:5080
├── playwright.config.ts               # webServer: API + Vite
├── e2e/catalog.spec.ts
└── src/
    ├── main.tsx, App.tsx              # router + layout only
    ├── styles.css
    ├── api/{client.ts, catalog.ts, types.ts}
    ├── lib/money.ts
    ├── components/{BookCard.tsx, Pagination.tsx, StatusMessage.tsx}
    └── pages/{CatalogPage.tsx, BookDetailPage.tsx, NotFoundPage.tsx}
```

**Structure Decision**: single API project with feature folders (Constitution VII), one test project, one web app.
Legacy `Controllers/`, `Data/`, `Models/`, `Utils/` are deleted (FR-012).

## Complexity Tracking

> No violations. Table intentionally empty.
