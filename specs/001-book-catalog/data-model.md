# Data Model: Book Catalog

## Entity: Book (`Features/Catalog/Book.cs`, table `Books`)

| Field | C# type | PostgreSQL | Rules |
|---|---|---|---|
| Id | `int` | `integer` identity, PK | generated |
| Isbn | `string` | `varchar(13)`, unique index | exactly 13 digits |
| Title | `string` | `varchar(200)` not null | required |
| Author | `string` | `varchar(150)` not null | required |
| Genre | `string` | `varchar(50)` not null, index | required; one of the seeded genres |
| Price | `decimal` | `numeric(10,2)` not null | ≥ 0; INR; **never `double`** (Constitution IV) |
| PublishedYear | `int` | `integer` not null | 1450 – current year |
| StockQuantity | `int` | `integer` not null | ≥ 0 |
| Description | `string` | `varchar(2000)` not null | may be empty |

Derived (not stored): `InStock = StockQuantity > 0`.

### Changes from the legacy `Models/Book.cs`

| Legacy | New | Why |
|---|---|---|
| `title` | `Title` | naming convention |
| `ISBN` | `Isbn` | naming convention |
| `Category` | `Genre` | ubiquitous language in spec |
| `double Price` | `decimal Price` | Constitution IV |
| `stock` | `StockQuantity` | clarity |
| `Desc` | `Description` | clarity |
| `Year` | `PublishedYear` | clarity |

## DTOs (`Features/Catalog/CatalogContracts.cs`)

```text
BookSummary   { id, title, author, genre, price, inStock }
BookDetail    { id, isbn, title, author, genre, price, publishedYear, stockQuantity, inStock, description }
PagedResult<T>{ items: T[], page, pageSize, totalCount, totalPages }
CatalogQuery  { search?, genre?, sort = "title", page = 1, pageSize = 12 }
```

`totalPages = ceil(totalCount / pageSize)`; `0` when `totalCount = 0`.

## Sort keys

| `sort` | Ordering |
|---|---|
| `title` (default) | Title ascending |
| `price_asc` | Price ascending, then Title |
| `price_desc` | Price descending, then Title |
| `newest` | PublishedYear descending, then Title |

## State transitions

None — the catalog is read-only in this feature.
