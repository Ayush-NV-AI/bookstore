---

description: "Task list for 001-book-catalog"
---

# Tasks: Book Catalog

**Input**: Design documents from `/specs/001-book-catalog/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/catalog.openapi.yaml, quickstart.md

**Tests**: MANDATORY for every story (Constitution V). Test tasks come first in each story and must fail before implementation.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: US1 browse · US2 search/filter/sort · US3 detail

---

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 Delete legacy `src/BookStore.Api/Controllers/`, `Data/`, `Models/`, `Utils/` (FR-012, research R2)
- [X] T002 Update `src/BookStore.Api/BookStore.Api.csproj`: `<Nullable>enable</Nullable>`, remove Newtonsoft.Json (Constitution VII)
- [X] T003 [P] Create `tests/BookStore.Api.Tests/BookStore.Api.Tests.csproj` (xUnit, Mvc.Testing, EF InMemory — research R8) and add it to `BookStore.sln`
- [X] T004 [P] Add `react-router-dom` and `@playwright/test` to `web/package.json`; enable `strict` in `web/tsconfig.json`; add `test:e2e` script (research R6, R7)
- [X] T005 [P] Add `/api` and `/health` proxy to `web/vite.config.ts`; create `web/playwright.config.ts` with API + Vite `webServer`
- [X] T006 Move secrets out: strip `ConnectionStrings` and `PaymentGateway` from `appsettings.json`; create `appsettings.Development.json` with the local Docker connection string `BookStore`; set `docker-compose.yml` password to the local-dev value (Constitution VI)

---

## Phase 2: Foundational (Blocking Prerequisites)

- [X] T007 Create `Features/Catalog/Book.cs` with `BookConfiguration` (decimal `numeric(10,2)` price, unique ISBN, genre index) per data-model.md
- [X] T008 Create `Infrastructure/BookStoreDbContext.cs` applying configurations from the assembly
- [X] T009 Create `Infrastructure/SeedBooks.cs` (30 books, 8 genres, one out of stock) and `Infrastructure/DbSeeder.cs` (`EnsureCreatedAsync` + seed once)
- [X] T010 Rewrite `Program.cs`: DbContext (skipped in `Testing` env), `AddProblemDetails`, `UseExceptionHandler`, CORS `http://localhost:5173` in Development only, seeding, `/health` (FR-013), `public partial class Program`
- [X] T011 [P] Create `tests/BookStore.Api.Tests/BookStoreApiFactory.cs` (Testing env + InMemory database per factory)
- [X] T012 [P] Create `web/src/api/client.ts` (`request<T>`, `ApiError` with Problem Details) and `web/src/api/types.ts` mirroring the contract
- [X] T013 [P] Create `web/src/lib/money.ts` `formatInr()` (en-IN, INR) and `web/src/styles.css`
- [X] T014 Replace `web/src/App.tsx` with router + layout only; wrap in `BrowserRouter` in `web/src/main.tsx`

**Checkpoint**: `dotnet build` and `npm --prefix web run build` both green.

---

## Phase 3: User Story 1 - Browse the catalog (Priority: P1) 🎯 MVP

**Goal**: paginated, title-sorted grid with price and availability.

**Independent Test**: 12 cards, "30 books", "Page 1 of 3", Next → page 2.

### Tests for User Story 1 ⚠️ write first, watch them fail

- [X] T015 [P] [US1] API tests in `tests/BookStore.Api.Tests/Catalog/CatalogApiTests.cs`: default page, page 3, past-last-page, invalid page/pageSize → 400 problem+json, out-of-stock flagged, prices undiscounted
- [X] T016 [P] [US1] Playwright tests in `web/e2e/catalog.spec.ts`: first page, Next + URL, out-of-stock card

### Implementation for User Story 1

- [X] T017 [US1] Create `Features/Catalog/CatalogContracts.cs`: `BookSummary`, `BookDetail`, `PagedResult<T>`, `CatalogSort`, `CatalogQuery` (DataAnnotations + `IValidatableObject`)
- [X] T018 [US1] Implement `ICatalogService.ListBooksAsync` in `Features/Catalog/CatalogService.cs` (AsNoTracking, count, order, Skip/Take, project to DTO)
- [X] T019 [US1] Implement `GET /api/v1/catalog/books` in `Features/Catalog/CatalogController.cs`; register `ICatalogService` in `Program.cs`
- [X] T020 [P] [US1] Create `web/src/components/BookCard.tsx`, `Pagination.tsx`, `StatusMessage.tsx`
- [X] T021 [US1] Create `web/src/api/catalog.ts` `listBooks()` and `web/src/pages/CatalogPage.tsx` with loading / error+retry / empty states (FR-011) and URL-driven page (FR-010)

**Checkpoint**: US1 tests green.

---

## Phase 4: User Story 2 - Search, filter and sort (Priority: P1)

**Goal**: combined search, genre and sort reflected in the URL.

**Independent Test**: "le guin" → 2 books; Technology + price_desc → Domain-Driven Design first.

### Tests for User Story 2 ⚠️

- [X] T022 [P] [US2] API tests: search any case, trimmed search, SQL metacharacters as plain text, >100 chars → 400, genre + price_desc, unknown genre → empty, unknown sort → 400, genres list
- [X] T023 [P] [US2] Playwright tests: search by author, genre + sort, empty state + Clear filters

### Implementation for User Story 2

- [X] T024 [US2] Add search (`ToLower().Contains`, research R3), genre and sort handling to `CatalogService.ListBooksAsync`
- [X] T025 [US2] Implement `ICatalogService.ListGenresAsync` and `GET /api/v1/catalog/genres`
- [X] T026 [US2] Add search form, genre select, sort select and Clear filters to `CatalogPage.tsx`; `listGenres()` in `web/src/api/catalog.ts`

**Checkpoint**: US1 + US2 tests green.

---

## Phase 5: User Story 3 - View book details (Priority: P2)

**Goal**: detail page with back navigation that restores filters.

**Independent Test**: Dune shows ISBN 9780001000001 and ₹599.00; unknown id shows "Book not found".

### Tests for User Story 3 ⚠️

- [X] T027 [P] [US3] API tests: existing book detail, unknown id → 404 problem+json
- [X] T028 [P] [US3] Playwright tests: open Dune, Back to catalog restores filters, unknown id

### Implementation for User Story 3

- [X] T029 [US3] Implement `ICatalogService.GetBookAsync` and `GET /api/v1/catalog/books/{id:int}` returning `Problem(404)` when missing
- [X] T030 [US3] Create `web/src/pages/BookDetailPage.tsx` (`getBook()`, not-found vs error states, back link using router state) and `NotFoundPage.tsx`

**Checkpoint**: all stories green.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T031 [P] API tests: every legacy route returns 404 (FR-012, SC-004); `/health` returns 200 (FR-013)
- [X] T032 [P] Update `docs/context/architecture.md` and `src/BookStore.Api/GEMINI.md` — legacy folders are gone
- [X] T033 Run `quickstart.md` end to end

---

## Phase 7: Convergence

*Appended by `/speckit.converge` after the first `/speckit.implement` run.*

- [X] T034 [US2] FR-010 partly met: genre and sort changes kept the old `page` value — reset `page` on any filter change in `CatalogPage.tsx` `update()`
- [X] T035 [US3] Acceptance scenario US3-2 had no implementation: `BookCard` must pass the current query string as router state so `BookDetailPage` can restore it

---

## Dependencies & Execution Order

- Setup (T001–T006) → Foundational (T007–T014) → stories.
- US1 → US2 (US2 extends the same service method and page). US3 depends only on Foundational and T017.
- Within a story: tests first (must fail), then contracts → service → controller → UI.

## Parallel Opportunities

- T003, T004, T005 · T011, T012, T013 · T015 + T016 · T022 + T023 · T027 + T028 · T020 alongside T018.
