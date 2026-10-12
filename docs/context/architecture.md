# Architecture

## Legacy (removed by spec 001 — never reintroduce these patterns)

The first version was vibe-coded in a weekend. Spec `001-book-catalog` deleted these files. They are listed
so that nobody — human or agent — rebuilds them:

| Legacy file | Problem |
|---|---|
| `src/BookStore.Api/Controllers/BooksController.cs` | Controller talks to `DbContext` directly, unversioned routes (`/api/getBooks`), SQL built by string concatenation, exceptions swallowed |
| `src/BookStore.Api/Utils/Helper.cs` | God class, money as `double`, hard-coded key |
| `src/BookStore.Api/Models/Book.cs` | Inconsistent property casing, `double Price` |
| `web/src/App.tsx` | One 150-line component, `any` everywhere, hard-coded API URL, client-side price maths |

## Target architecture (all new code)

```
src/BookStore.Api/
  Program.cs                      composition root only
  Infrastructure/                 DbContext, seeding, cross-cutting setup
  Features/<Feature>/             one folder per feature (vertical slice)
    <Entity>.cs                   EF Core entity
    <Feature>Controller.cs        thin HTTP adapter, route prefix /api/v1/<feature>
    <Feature>Service.cs           business rules, behind I<Feature>Service
    <Feature>Contracts.cs         request/response DTOs (records)
tests/BookStore.Api.Tests/        xUnit API tests via WebApplicationFactory
web/src/
  api/                            typed fetch clients, one file per feature
  pages/                          route-level components
  components/                     reusable presentational components
  lib/                            pure helpers (formatting)
web/e2e/                          Playwright UI tests
specs/<###-feature>/              Spec Kit artifacts (spec, plan, research, data-model, contracts, tasks)
```

## Runtime topology

| Component | Local address | Notes |
|---|---|---|
| PostgreSQL 16 | `localhost:5432`, db `bookstore` | Docker Desktop, volume `pgdata` |
| API | `http://localhost:5080` | `/health` for readiness |
| Web (dev) | `http://localhost:5173` | Vite proxies `/api` to the API |
| API (container, profile `full`) | `http://localhost:5080` → `api:8080` | non-root `aspnet:10.0` image |
| Web (container, profile `full`) | `http://localhost:8080` | nginx serves the SPA and proxies `/api` to `api:8080` |

## Features

| Feature | Folder | Routes | Spec |
|---|---|---|---|
| Catalog | `Features/Catalog` | `/api/v1/catalog/*` | `specs/001-book-catalog` |
| Cart | `Features/Carts` | `/api/v1/carts/*` | `specs/002-orders-cart` |
| Orders | `Features/Orders` | `/api/v1/orders/*` | `specs/002-orders-cart` |

Business-rule failures are thrown as `NotFoundException` / `ConflictException` / `DomainValidationException`
(`Infrastructure/DomainExceptions.cs`) and mapped to Problem Details by `DomainExceptionHandler`.

## Data

- The database schema is created by `EnsureCreated()` at start-up (training simplification).
  A schema change therefore needs `docker compose down -v` to reset the volume.
- Seed data: 30 books across 8 genres, prices in INR.
