# Data Model: Orders and Cart

## Cart (`Features/Carts/Cart.cs`, table `Carts`)

| Field | C# | PostgreSQL | Rules |
|---|---|---|---|
| Id | `Guid` | `uuid` PK | server-generated |
| CreatedAt | `DateTimeOffset` | `timestamptz` | UTC |
| UpdatedAt | `DateTimeOffset` | `timestamptz` | UTC, touched on every change |
| Items | `List<CartItem>` | — | cascade delete |

## CartItem (table `CartItems`)

| Field | C# | PostgreSQL | Rules |
|---|---|---|---|
| Id | `int` | identity PK | |
| CartId | `Guid` | FK → Carts | |
| BookId | `int` | FK → Books (restrict) | unique with CartId |
| Quantity | `int` | `integer` | 1–10, ≤ stock when set |
| Book | `Book` | navigation | |

## Order (`Features/Orders/Order.cs`, table `Orders`)

| Field | C# | PostgreSQL | Rules |
|---|---|---|---|
| Id | `Guid` | `uuid` PK | |
| OrderNumber | `string` | `varchar(20)` unique | `BS-yyyyMMdd-nnnnnn` |
| CustomerEmail | `string` | `varchar(254)` index | valid email, stored lower-case |
| Status | `OrderStatus` | `varchar(20)` | `Placed` (only state in this feature) |
| PlacedAt | `DateTimeOffset` | `timestamptz` | UTC |
| Total | `decimal` | `numeric(10,2)` | = Σ LineTotal |
| Lines | `List<OrderLine>` | — | cascade delete |

## OrderLine (table `OrderLines`)

| Field | C# | PostgreSQL | Rules |
|---|---|---|---|
| Id | `int` | identity PK | |
| OrderId | `Guid` | FK → Orders | |
| BookId | `int` | `integer` | no FK — the line is a historical snapshot |
| Title | `string` | `varchar(200)` | snapshot |
| UnitPrice | `decimal` | `numeric(10,2)` | snapshot |
| Quantity | `int` | `integer` | 1–10 |
| LineTotal | `decimal` | `numeric(10,2)` | UnitPrice × Quantity |

## DTOs

```text
CartView        { id, items: CartLineView[], itemCount, subtotal }
CartLineView    { bookId, title, unitPrice, quantity, lineTotal }
SetQuantity     { quantity }                               // 1..10
PlaceOrder      { cartId, customerEmail }
OrderView       { id, orderNumber, status, customerEmail, placedAt, total, lines: OrderLineView[] }
OrderLineView   { bookId, title, unitPrice, quantity, lineTotal }
```

## State transitions

```text
Cart:  (none) --POST /carts--> Open --PUT/DELETE items--> Open --POST /orders--> (deleted)
Order: (none) --POST /orders--> Placed
```
