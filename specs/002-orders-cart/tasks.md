---

description: "Task list for 002-orders-cart — owner column drives the Day 3 subagent fan-out"
---

# Tasks: Orders and Cart

**Input**: Design documents from `/specs/002-orders-cart/`

**Prerequisites**: plan.md (incl. Agent Orchestration), spec.md, research.md, data-model.md, contracts/orders-cart.openapi.yaml

**Tests**: MANDATORY (Constitution V). Each agent writes its tests first.

## Format: `[ID] [P?] [Story] (owner) Description`

---

## Phase 1: Setup (parent)

- [X] T001 (parent) Confirm 001 is green: `dotnet test` and `npm --prefix web run test:e2e`
- [X] T002 (parent) Create `.gemini/agents/` definitions for the four subagents in plan.md and verify with `/agents list`

## Phase 2: Foundational (parent, sequential — blocks the fan-out)

- [X] T003 (parent) `Infrastructure/DomainExceptions.cs`: `NotFoundException`, `ConflictException`, `DomainValidationException`
- [X] T004 (parent) `Infrastructure/DomainExceptionHandler.cs` (`IExceptionHandler` → Problem Details 404/409/400); register `AddExceptionHandler<DomainExceptionHandler>()` in `Program.cs`
- [X] T005 [P] (parent) `Features/Carts/Cart.cs`: `Cart`, `CartItem` + `IEntityTypeConfiguration` per data-model.md
- [X] T006 [P] (parent) `Features/Orders/Order.cs`: `Order`, `OrderLine`, `OrderStatus` + configurations per data-model.md
- [X] T007 (parent) Add `Carts`, `CartItems`, `Orders`, `OrderLines` DbSets to `BookStoreDbContext`
- [X] T008 (parent) `docker compose down -v && docker compose up -d db`; `dotnet build` green

**Checkpoint**: foundation committed → fan out Phases 3–5 **in parallel**.

---

## Phase 3: User Story 1 — Cart API (@cart-api-builder) 🎯

- [X] T009 [P] [US1] (@cart-api-builder) Tests first: `tests/BookStore.Api.Tests/Carts/CartApiTests.cs` — create 201 + Location, set quantity totals, set replaces not adds, quantity 0/11 → 400, above stock → 409, unknown book/cart → 404, remove idempotent
- [X] T010 [US1] (@cart-api-builder) `Features/Carts/CartContracts.cs`: `CartView`, `CartLineView`, `SetQuantityRequest` (`[Range(1,10)]`)
- [X] T011 [US1] (@cart-api-builder) `Features/Carts/CartService.cs`: `ICartService` Create / Get / SetQuantity / Remove; totals in `decimal`
- [X] T012 [US1] (@cart-api-builder) `Features/Carts/CartController.cs`: routes per contract, `CreatedAtAction` for POST
- [X] T013 [US1] (@cart-api-builder) `dotnet test --filter FullyQualifiedName~Cart` green (ignore build errors that are only in `Features/Orders/**` — report them)
- [X] T014 [US1] (@cart-api-builder) Return: files changed, test result, any contract ambiguity found

## Phase 4: User Story 2 + 3 — Orders API (@orders-api-builder)

- [X] T015 [P] [US2] (@orders-api-builder) Tests first: `tests/BookStore.Api.Tests/Orders/OrdersApiTests.cs` — place order decrements stock + snapshots + deletes cart, empty cart 400, bad email 400, unknown cart 404, stock taken by another order 409, get order, list by email newest first, unknown order 404
- [X] T016 [US2] (@orders-api-builder) `Features/Orders/OrderContracts.cs`: `PlaceOrderRequest`, `OrderView`, `OrderLineView`, `OrderLookupQuery`
- [X] T017 [US2] (@orders-api-builder) `Features/Orders/OrderService.cs`: `IOrderService.PlaceAsync` (re-check stock, snapshot, decrement, delete cart, one SaveChanges — research R4), `GetAsync`, `ListByEmailAsync`
- [X] T018 [US2] (@orders-api-builder) `Features/Orders/OrdersController.cs`: routes per contract, `CreatedAtAction`
- [X] T019 [US3] (@orders-api-builder) `dotnet test --filter FullyQualifiedName~Orders` green (ignore errors only in `Features/Carts/**` — report them)
- [X] T020 [US2] (@orders-api-builder) Return: files changed, test result, any contract ambiguity found

## Phase 5: Storefront UI (@storefront-ui-builder)

- [X] T021 [P] [US1] (@storefront-ui-builder) Tests first: `web/e2e/checkout.spec.ts` — add to cart + header count, change quantity + subtotal, remove → empty, out-of-stock add disabled, checkout → confirmation + count 0
- [X] T022 [P] [US1] (@storefront-ui-builder) `web/src/api/types.ts` cart/order types; `web/src/api/cart.ts`; `web/src/api/orders.ts`; `request()` must support POST/PUT/DELETE
- [X] T023 [US1] (@storefront-ui-builder) `web/src/cart/CartContext.tsx`: cart id in `localStorage`, recreate on 404, `add`, `setQuantity`, `remove`, `clear`
- [X] T024 [US1] (@storefront-ui-builder) Header cart link with count (`nav-cart-count`) in `App.tsx`; wrap app in `CartProvider` in `main.tsx`
- [X] T025 [US1] (@storefront-ui-builder) "Add to cart" on `BookDetailPage.tsx` (disabled when out of stock, shows 409 message)
- [X] T026 [US1] (@storefront-ui-builder) `web/src/pages/CartPage.tsx` (`/cart`)
- [X] T027 [US2] (@storefront-ui-builder) `web/src/pages/CheckoutPage.tsx` (`/checkout`), Problem Details messages shown
- [X] T028 [US3] (@storefront-ui-builder) `web/src/pages/OrderConfirmationPage.tsx` (`/orders/:id`)
- [X] T029 (@storefront-ui-builder) `npm --prefix web run build` green; return files changed

---

## Phase 6: Integration and containerisation (parent, after all agents return)

- [X] T030 (parent) Register `ICartService` and `IOrderService` in `Program.cs`; remove the agents' TEMPORARY WIRING from the test classes; `dotnet test` (all) green
- [X] T031 (parent) `npm --prefix web run test:e2e` (all) green
- [X] T032 [P] (parent) `src/BookStore.Api/Dockerfile` + `.dockerignore` (sdk:10.0 → aspnet:10.0, non-root, port 8080)
- [X] T033 [P] (parent) `web/Dockerfile` + `nginx.conf` + `.dockerignore` (proxy `/api`, `/health` → `api:8080`, SPA fallback)
- [X] T034 (parent) `docker-compose.yml`: `api` and `web` under profile `full`; `db/init/01-readonly-role.sql` mounted
- [X] T035 (parent) `docker compose --profile full up -d --build`; `E2E_BASE_URL=http://localhost:8080 npm --prefix web run test:e2e` green
- [X] T036 (parent) MCP check: ask the agent to list the four new tables through the read-only `bookstore-db` server

## Phase 7: Audit

- [X] T037 (@contract-auditor) Compare controllers and DTOs to `contracts/orders-cart.openapi.yaml` and the constitution; report mismatches only

## Dependencies

- Phase 2 blocks 3, 4, 5. Phases 3, 4, 5 are independent of each other (contract is the seam).
- Phase 6 needs 3, 4, 5. Phase 7 needs 6.
