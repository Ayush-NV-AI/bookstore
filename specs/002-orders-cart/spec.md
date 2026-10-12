# Feature Specification: Orders and Cart

**Feature Branch**: `002-orders-cart`

**Created**: 2026-10-14

**Status**: Ratified — ready for implementation (pre-ratified by the trainer for Day 3 AM; produced with the same `/speckit.specify` → `/speckit.clarify` → `/speckit.plan` → `/speckit.tasks` → `/speckit.analyze` flow as 001)

**Input**: User description: "Shoppers add books to a cart, change quantities, remove lines and check out with an
email address. Checkout creates an order with the prices at that moment, reduces stock and empties the cart. A
shopper can see the confirmation and look up their orders by email."

## Clarifications

### Session 2026-10-14

- Q: Do shoppers need an account? → A: No. The cart is anonymous, identified by an id the browser keeps. Orders are looked up by email.
- Q: Maximum quantity per line? → A: 10. Quantity 1–10; to remove a line use remove, not quantity 0.
- Q: Which price does the cart show — the price when added, or the current price? → A: The current catalog price. The **order** snapshots the price at checkout.
- Q: When is stock checked? → A: When a quantity is set (reject above stock) **and** again at checkout (another shopper may have bought the last copies).
- Q: What happens to the cart after checkout? → A: It is deleted. The browser starts a new cart on the next add.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build a cart (Priority: P1)

A shopper adds a book to the cart from its detail page, sees the cart count in the header, opens the cart,
changes quantities and removes lines. Line totals and the subtotal are computed by the server.

**Why this priority**: no checkout without a cart.

**Independent Test**: add "Pride and Prejudice" ×2 → cart shows one line, `₹500.00` subtotal, header count 2.

**Acceptance Scenarios**:

1. **Given** a book in stock, **When** the shopper selects "Add to cart", **Then** the header cart count increases and the cart contains the book with quantity 1.
2. **Given** a cart line, **When** the quantity is changed to 2, **Then** the line total and subtotal are recalculated by the server.
3. **Given** a cart line, **When** the shopper removes it, **Then** the line disappears, and an empty cart shows "Your cart is empty".
4. **Given** a book with 4 copies, **When** the quantity is set to 5, **Then** the change is rejected with "Only 4 copies in stock".
5. **Given** an out-of-stock book, **When** its detail page is shown, **Then** "Add to cart" is disabled.

---

### User Story 2 - Check out (Priority: P1)

A shopper enters an email address and places the order. The order captures each line's title and unit price,
reduces stock, empties the cart and shows a confirmation with an order number.

**Why this priority**: this is the revenue path.

**Independent Test**: cart with one line → place order with `asha@example.com` → confirmation shows status
"Placed", the total, and the header cart count returns to 0.

**Acceptance Scenarios**:

1. **Given** a non-empty cart and a valid email, **When** the order is placed, **Then** an order with status "Placed" is created, stock is reduced by each line's quantity, and the cart is deleted.
2. **Given** an empty cart, **When** checkout is attempted, **Then** it is rejected with "Cart is empty".
3. **Given** an invalid email, **When** checkout is attempted, **Then** it is rejected with a validation error on the email field.
4. **Given** another shopper bought the last copies after this cart was filled, **When** checkout is attempted, **Then** it is rejected with a conflict naming the book, and nothing is charged or reserved.

---

### User Story 3 - Find my orders (Priority: P2)

A shopper sees an order's confirmation page by its id and can list their orders by email, newest first.

**Why this priority**: support and trust; not needed to take money.

**Independent Test**: place two orders for the same email → listing returns both, newest first.

**Acceptance Scenarios**:

1. **Given** a placed order, **When** its confirmation page is opened, **Then** order number, status, lines and total are shown.
2. **Given** two orders for one email, **When** orders are listed by that email, **Then** both are returned, newest first.
3. **Given** an unknown order id, **When** it is requested, **Then** the API returns 404 Problem Details.

### Edge Cases

- Setting the quantity of a book already in the cart replaces the quantity (it does not add to it).
- Removing a line that is not in the cart succeeds (idempotent).
- A stale cart id in the browser (cart deleted after checkout) silently starts a new cart.
- Prices in the cart change if the catalog price changes; the order total never changes after placement.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create an empty cart via `POST /api/v1/carts` and return it with status 201.
- **FR-002**: System MUST return a cart via `GET /api/v1/carts/{cartId}` with lines (book id, title, unit price, quantity, line total), item count and subtotal.
- **FR-003**: System MUST set a line's quantity (1–10) via `PUT /api/v1/carts/{cartId}/items/{bookId}`, rejecting quantities above current stock with 409.
- **FR-004**: System MUST remove a line via `DELETE /api/v1/carts/{cartId}/items/{bookId}`.
- **FR-005**: System MUST place an order via `POST /api/v1/orders` from a cart id and email, re-checking stock, snapshotting title and unit price per line, reducing stock and deleting the cart in one atomic save.
- **FR-006**: System MUST return an order via `GET /api/v1/orders/{orderId}` and list orders via `GET /api/v1/orders?customerEmail=`.
- **FR-007**: All monetary values MUST be computed on the server as `decimal` (Constitution IV).
- **FR-008**: The web app MUST show a header cart count, an Add to cart action on the book detail page, a cart page, a checkout page and an order confirmation page.
- **FR-009**: Errors MUST be Problem Details: 400 validation, 404 missing cart/book/order, 409 stock conflict.

### Key Entities

- **Cart**: an anonymous shopper's basket — id, created/updated time, lines.
- **Cart line**: a book and a quantity (1–10) in a cart; one line per book.
- **Order**: a placed purchase — id, order number, email, status, placed time, total, lines.
- **Order line**: snapshot of book id, title, unit price, quantity and line total at checkout.

## Out of Scope

- Payment capture, payment gateways and refunds (no money moves — Constitution VI keeps keys out anyway).
- User accounts, login and order history beyond lookup by email.
- Tax, shipping, discounts and coupons.
- Order cancellation, status changes after "Placed", fulfilment and email notifications.
- Stock reservation while items sit in a cart.
- Guarding against two simultaneous checkouts of the last copy beyond the atomic save (see research R4).

## Success Criteria *(mandatory)*

- **SC-001**: A shopper goes from book detail to order confirmation in under 60 seconds.
- **SC-002**: 100% of acceptance scenarios are covered by passing xUnit or Playwright tests.
- **SC-003**: No order total ever differs from the sum of its line totals.

## Assumptions

- Spec 001 (catalog) is merged; `Book.StockQuantity` is the stock of record.
- The browser keeps the cart id in `localStorage`.
