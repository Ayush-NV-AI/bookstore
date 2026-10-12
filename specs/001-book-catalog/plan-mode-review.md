# Plan Mode Review — Book Catalog approach

> Produced in Gemini CLI **Plan Mode** (`/plan`, read-only), opened with **Ctrl+X** in the external editor
> and annotated by hand by the architect. Lines starting `> ARCHITECT:` are human edits. This file is the
> input to `/speckit.plan`. Copied from `.gemini/plans/` for traceability.

## Goal

Implement spec `001-book-catalog`: versioned catalog API + React catalog page, remove legacy book endpoints.

## Findings from read-only exploration

1. `BooksController` uses `AppDb` directly and builds SQL by concatenation (`Search`). → injection risk.
2. `Book.Price` is `double`; `Helper.ApplyDiscount` mutates prices on read. → violates Constitution IV.
3. `App.tsx` hard-codes `http://localhost:5080` and recomputes discount + 18% tax client-side.
4. No test project exists. `BookStore.sln` has one project.
5. CORS is `AllowAnyOrigin()`.

## Proposed approach (model draft)

1. Create `Features/Catalog/` with `Book`, `CatalogController`, `CatalogService`, DTO records.
2. Replace `AppDb` with `BookStoreDbContext` in `Infrastructure/`; move seed data to `DbSeeder`.
3. Add a `Repository<T>` generic repository and a `UnitOfWork` over EF Core.

> ARCHITECT: **Reject step 3.** EF Core's DbContext already is the unit of work and DbSet the repository.
> Constitution VII (Simplicity) — an extra abstraction an agent adds is one a human maintains. Services use
> the DbContext directly.

4. Use `EF.Functions.ILike` for search.

> ARCHITECT: **Change.** ILike is Npgsql-only and our API tests run on the EF InMemory provider. Use
> `ToLower().Contains()` — translated to a parameterised `lower(..) LIKE` on PostgreSQL. Record the
> trade-off in research.md (R3). Revisit with a trigram index only if SC-002 fails.

5. Add Swashbuckle for Swagger UI.

> ARCHITECT: **Reject.** The hand-reviewed contract in `contracts/catalog.openapi.yaml` is the source of
> truth (Constitution II); a generated Swagger UI adds a dependency and a second, drifting description.
> One fewer package (Constitution VII). Contract conformance is proven by the API tests instead.

6. React: add React Router, a `CatalogPage`, `BookDetailPage`, typed API client, URL-driven state.
7. Tests: new `tests/BookStore.Api.Tests` (xUnit, `WebApplicationFactory`, InMemory provider) and
   Playwright in `web/e2e`.

> ARCHITECT: **Add.** Playwright runs against the real PostgreSQL container, not InMemory — the UI test is
> the one place we prove the real query translation works. Add `webServer` entries so `npm run test:e2e`
> starts the API and Vite itself.

8. Delete `Controllers/`, `Utils/`, `Models/`, `Data/` legacy folders.

> ARCHITECT: **Approve**, and tighten CORS to `http://localhost:5173` in Development only (security.md).
> Also: the schema changes, so quickstart must tell people to run `docker compose down -v` first.

## Approval

Approved with the edits above → proceed with `/speckit.plan`, implementation **not** started from Plan Mode.
