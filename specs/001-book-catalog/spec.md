# Feature Specification: Book Catalog

**Feature Branch**: `001-book-catalog`

**Created**: 2026-10-13

**Status**: Clarified — ready for planning

**Input**: User description: "Replace the vibe-coded book listing with a proper Book Catalog: a versioned
API and a React catalog page where shoppers browse, search, filter and sort books and open a book's
detail page. Prices are shown exactly as stored. The legacy book endpoints and the client-side discount
logic go away."

## Clarifications

### Session 2026-10-13

- Q: What page size should the catalog use, and is it adjustable? → A: Default 12, client may request 1–48. Outside that range is a validation error.
- Q: How should search match? → A: Case-insensitive substring match on title **or** author. Input is trimmed; an empty search means no filter; maximum 100 characters.
- Q: What happens to the legacy 10% / 5% discounts? → A: Removed. The catalog shows the stored list price only. Promotions are a separate future feature (out of scope).
- Q: Are out-of-stock books hidden? → A: No. They are listed and labelled "Out of stock"; they are never hidden.
- Q: What does a page number past the last page return? → A: An empty item list with correct totals (not an error). Page < 1 is a validation error.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse the catalog (Priority: P1)

A shopper opens the store and sees the catalog as a grid of books, a page at a time, sorted by title,
with each book's title, author, genre, price and availability.

**Why this priority**: Without browsing there is no store. This is the MVP slice.

**Independent Test**: Seed 30 books, open the catalog, verify 12 cards on page 1, the total count of 30,
and that "Next" moves to page 2 of 3.

**Acceptance Scenarios**:

1. **Given** 30 books in the catalog, **When** the shopper opens the catalog, **Then** 12 books are shown, sorted by title, with the text "30 books" and "Page 1 of 3".
2. **Given** the shopper is on page 1, **When** they select "Next", **Then** page 2 is shown and the URL contains `page=2`.
3. **Given** a book with zero stock, **When** it appears in the list, **Then** it is labelled "Out of stock" and its price is still shown.
4. **Given** the API is unreachable, **When** the catalog loads, **Then** an error message with a "Retry" action is shown instead of an empty grid.

---

### User Story 2 - Search, filter and sort (Priority: P1)

A shopper narrows the catalog by typing part of a title or author, choosing a genre and choosing a sort
order. The choices combine and are reflected in the URL so the view can be shared.

**Why this priority**: 30 books is a demo; a real catalog is unusable without search.

**Independent Test**: Search "le guin" returns exactly the two Le Guin titles; genre "Technology" sorted
by price descending lists "Domain-Driven Design" first.

**Acceptance Scenarios**:

1. **Given** the catalog, **When** the shopper searches "le guin", **Then** only books whose title or author contains that text (any case) are shown.
2. **Given** the catalog, **When** the shopper selects genre "Technology" and sort "Price: high to low", **Then** only Technology books are shown, most expensive first.
3. **Given** a search with no matches, **When** results load, **Then** the message "No books match your search" and a "Clear filters" action are shown.
4. **Given** a search of 101 characters, **When** it is submitted to the API, **Then** the API responds with a validation error.

---

### User Story 3 - View book details (Priority: P2)

A shopper opens a book and sees its full description, ISBN, publication year, price and stock status,
and can return to the catalog where they left it.

**Why this priority**: Needed before a cart exists (spec 002), but browsing delivers value without it.

**Independent Test**: Open "Dune" from the grid; the detail page shows ISBN `9780001000001`, `₹599.00` and "In stock".

**Acceptance Scenarios**:

1. **Given** the catalog, **When** the shopper selects a book, **Then** its detail page shows description, ISBN, year, price and availability.
2. **Given** a detail page reached from a filtered catalog, **When** the shopper selects "Back to catalog", **Then** the same filters and page are restored.
3. **Given** a book id that does not exist, **When** the detail page is requested, **Then** the page shows "Book not found" and the API returns 404 Problem Details.

### Edge Cases

- Search text with SQL metacharacters (`' OR 1=1 --`) is treated as plain text and returns no rows.
- Leading/trailing whitespace in search is ignored; whitespace-only equals no search.
- Unknown genre returns an empty list, not an error.
- Unknown sort value returns 400 with the list of allowed values.
- Page past the end returns 200 with `items: []` and correct `totalCount`/`totalPages`.
- `pageSize` of 0 or 49 returns 400.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose `GET /api/v1/catalog/books` returning a page of book summaries with `page`, `pageSize`, `totalCount`, `totalPages`.
- **FR-002**: System MUST default to page 1, page size 12 and sort by title ascending.
- **FR-003**: System MUST accept `pageSize` from 1 to 48 and `page` ≥ 1, and reject other values with 400 Problem Details.
- **FR-004**: System MUST support `search` as a case-insensitive substring match on title or author (trimmed, ≤ 100 characters).
- **FR-005**: System MUST support `genre` as a case-insensitive exact match.
- **FR-006**: System MUST support `sort` values `title`, `price_asc`, `price_desc`, `newest`, and reject others with 400.
- **FR-007**: System MUST expose `GET /api/v1/catalog/books/{id}` returning book details, or 404 Problem Details.
- **FR-008**: System MUST expose `GET /api/v1/catalog/genres` returning the distinct genres, alphabetically.
- **FR-009**: System MUST return prices exactly as stored (two decimal places), with no discount applied.
- **FR-010**: The web catalog MUST reflect search, genre, sort and page in the URL query string.
- **FR-011**: The web catalog MUST show loading, error (with retry) and empty states.
- **FR-012**: The legacy endpoints `/api/getBooks`, `/api/Book/{id}`, `/api/searchBooks`, `/api/addBook`, `/api/books/cheap` and `/api/books/category/{cat}` MUST be removed, together with the client-side discount and tax logic.
- **FR-013**: System MUST expose `GET /health` returning 200 when the API can reach the database.

### Key Entities

- **Book**: a title for sale — ISBN-13, title, author, genre, list price (INR), publication year, stock quantity, description.
- **Genre**: a label on a book used for filtering (not a separate managed entity in this feature).

## Out of Scope

- Cart, checkout, orders and payments (spec 002).
- Authentication, accounts and admin book management (create, edit, delete).
- Discounts, promotions, coupons and tax calculation.
- Book cover images, ratings, reviews and recommendations.
- Multi-currency and localisation beyond INR / en-IN formatting.
- Full-text search ranking, typo tolerance and search suggestions.
- EF Core migrations (schema is created at start-up for this training codebase).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A shopper finds a known book by author in under 10 seconds from opening the catalog.
- **SC-002**: Catalog list responses return in under 300 ms (p95) for 1,000 books on a developer laptop.
- **SC-003**: 100% of acceptance scenarios above are covered by a passing xUnit or Playwright test.
- **SC-004**: Zero legacy endpoints respond after release (all return 404).

## Assumptions

- Prices are stored in INR and displayed with `en-IN` formatting (`₹1,450.00`).
- The seed data set of 30 books across 8 genres is representative for testing.
- Shoppers are anonymous; no personalisation.
