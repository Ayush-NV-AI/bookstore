# Research: Orders and Cart

## R1 — Cart identity

- **Decision**: server-generated `Guid` cart id, kept in `localStorage` (`bookstore.cartId`).
- **Rationale**: anonymous shopping (clarification Q1); a Guid is unguessable enough for a training store.
- **Alternatives**: cookie session (rejected — needs session middleware and CSRF thinking); accounts (out of scope).

## R2 — Error signalling from services

- **Decision**: services throw `NotFoundException`, `ConflictException`, `DomainValidationException`; one `IExceptionHandler` maps them to Problem Details 404/409/400.
- **Rationale**: keeps controllers thin (Constitution III) and is built into ASP.NET Core (`AddExceptionHandler<T>`).
- **Alternatives**: `Result<T>` library (rejected — new dependency, Constitution VII); status codes in services (rejected — leaks HTTP into the domain).

## R3 — Price snapshot

- **Decision**: cart lines read the live `Book.Price`; order lines copy `Title` and `UnitPrice` at checkout.
- **Rationale**: clarification Q3; an order must never change after placement (SC-003).

## R4 — Atomic checkout

- **Decision**: one `SaveChangesAsync` that inserts the order, decrements stock and deletes the cart.
- **Rationale**: a single SaveChanges is one transaction on PostgreSQL and needs no explicit transaction API (which the InMemory test provider does not support).
- **Known limitation**: two simultaneous checkouts of the last copy can both pass the stock check. Fix later with a concurrency token on `Book.StockQuantity` (PostgreSQL `xmin`). Recorded in spec Out of Scope.

## R5 — Order number

- **Decision**: `BS-<yyyyMMdd>-<6 random digits>`, unique index.
- **Rationale**: human-readable for support calls; no sequence table needed.

## R6 — Containerisation

- **Decision**: multi-stage Dockerfiles (`sdk:10.0` → `aspnet:10.0` as non-root `app`; `node:22-alpine` → `nginx:alpine`); nginx proxies `/api` and `/health` to `api:8080`; compose profile `full` so `docker compose up -d db` still starts only the database for local development.
- **Rationale**: same-origin in the container removes CORS; the profile keeps the Day 1–2 workflow unchanged.

## R7 — Database access for the agent (MCP)

- **Decision**: MCP Toolbox for Databases (`npx @toolbox-sdk/server --prebuilt=postgres --stdio`) connecting as a **read-only** role `bookstore_ro`, tools allow-listed in `.gemini/settings.json`.
- **Rationale**: lets the agent verify the schema it created without giving it write access; the password comes from an environment variable (Constitution VI).
- **Alternatives**: giving the agent `psql` through the shell (rejected — full write access, no allow-list).
