# Implementation Plan: Orders and Cart

**Branch**: `002-orders-cart` | **Date**: 2026-10-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-orders-cart/spec.md`

## Summary

Add two feature slices — `Cart` and `Orders` — to the API, sharing a small domain-exception → Problem Details
mapping, plus storefront pages in React. Built by **three parallel subagents** after the parent session lays the
shared foundation (entities, DbSets, exception handling). Then the full stack is containerised.

## Technical Context

**Language/Version**: C# 14 / .NET 10; TypeScript 5.9

**Primary Dependencies**: unchanged from 001 — no new packages

**Storage**: PostgreSQL 16; new tables `Carts`, `CartItems`, `Orders`, `OrderLines`

**Testing**: xUnit API tests per controller; Playwright `e2e/checkout.spec.ts`

**Target Platform**: developer laptop; full stack in Docker Compose (`--profile full`)

**Project Type**: Web application (API + SPA)

**Constraints**: money `decimal`; atomic checkout via a single `SaveChangesAsync`; no new dependencies

**Scale/Scope**: 7 endpoints, 3 new pages, 4 tables

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-research | Post-design |
|---|---|---|---|
| I Specification Before Code | spec ratified with Out of Scope; every task traces to FR/US | PASS | PASS |
| II Contract-First APIs | `contracts/orders-cart.openapi.yaml` before code; `/api/v1/carts`, `/api/v1/orders`; DTOs; Problem Details | PASS | PASS |
| III Layered Backend | controllers → `ICartService` / `IOrderService` → DbContext; domain exceptions mapped centrally | PASS | PASS |
| IV Money Is Decimal | `UnitPrice`, `LineTotal`, `Subtotal`, `Total` are `decimal` / `numeric(10,2)`; enforced by the BeforeTool hook | PASS | PASS |
| V Test-Gated Delivery | Cart + Orders API tests; checkout Playwright journey | PASS | PASS |
| VI Secrets Stay Out | MCP read-only DB password via `$BOOKSTORE_RO_PASSWORD`, not in `settings.json` | PASS | PASS |
| VII Simplicity | no new packages; no repository layer; exceptions + one handler instead of a Result library | PASS | PASS |

## Agent Orchestration (Day 3 AM)

Subagents **cannot call other subagents**, so the topology is a flat fan-out from the parent session.

```text
                 parent session (you)
     Phase 2 foundation — sequential, parent only
                         │
      ┌──────────────────┼────────────────────┐
      ▼                  ▼                    ▼
 @cart-api-builder  @orders-api-builder  @storefront-ui-builder     ← parallel, isolated context windows
 Features/Carts/**  Features/Orders/**   web/**
 tests/.../Carts/** tests/.../Orders/**
      └──────────────────┼────────────────────┘
                         ▼
     Phase 6 integration — parent only: Program.cs registrations, full test run
                         ▼
                 @contract-auditor (read-only)
```

| Owner | Tasks | May write | Must not touch |
|---|---|---|---|
| parent | T001–T008, T030–T036 | `Infrastructure/**`, entities, `Program.cs`, compose, Dockerfiles | — |
| @cart-api-builder | T009–T014 | `Features/Carts/CartContracts.cs`, `CartService.cs`, `CartController.cs`, `tests/BookStore.Api.Tests/Carts/**` | `Program.cs`, other features, `web/**` |
| @orders-api-builder | T015–T020 | `Features/Orders/OrderContracts.cs`, `OrderService.cs`, `OrdersController.cs`, `tests/BookStore.Api.Tests/Orders/**` | `Program.cs`, other features, `web/**` |
| @storefront-ui-builder | T021–T029 | `web/src/**`, `web/e2e/checkout.spec.ts` | `src/**`, `tests/**` |
| @contract-auditor | T037 | nothing (read-only tools) | everything |

## Project Structure

### Documentation (this feature)

```text
specs/002-orders-cart/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/orders-cart.openapi.yaml
└── checklists/requirements.md
```

### Source Code (repository root)

```text
src/BookStore.Api/
├── Infrastructure/DomainExceptions.cs          # NotFoundException, ConflictException, DomainValidationException
├── Infrastructure/DomainExceptionHandler.cs    # IExceptionHandler → Problem Details
├── Features/Carts/{Cart.cs, CartContracts.cs, CartService.cs, CartController.cs}
├── Features/Orders/{Order.cs, OrderContracts.cs, OrderService.cs, OrdersController.cs}
└── Dockerfile, .dockerignore
tests/BookStore.Api.Tests/{Carts/CartApiTests.cs, Orders/OrdersApiTests.cs}
web/
├── src/api/{cart.ts, orders.ts}
├── src/cart/CartContext.tsx
├── src/pages/{CartPage.tsx, CheckoutPage.tsx, OrderConfirmationPage.tsx}
├── e2e/checkout.spec.ts
└── Dockerfile, nginx.conf, .dockerignore
db/init/01-readonly-role.sql                    # read-only role for the MCP server
docker-compose.yml                              # db (default) + api, web (profile "full")
```

**Structure Decision**: two new feature folders, same layering as Catalog.

## Complexity Tracking

> No violations.
