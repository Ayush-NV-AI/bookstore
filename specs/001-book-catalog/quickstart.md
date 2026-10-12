# Quickstart: Book Catalog

Validates spec 001 end to end on a developer laptop. Run from the repository root.

## 1. Reset the database (schema changed from the legacy model)

```powershell
docker compose down -v
docker compose up -d db
```

## 2. Run the API

```powershell
dotnet run --project src/BookStore.Api
# → Now listening on: http://localhost:5080
```

## 3. Smoke-test the contract

```powershell
curl "http://localhost:5080/health"                                             # 200
curl "http://localhost:5080/api/v1/catalog/books"                               # totalCount 30, 12 items
curl "http://localhost:5080/api/v1/catalog/books?search=le%20guin"              # 2 items
curl "http://localhost:5080/api/v1/catalog/books?genre=Technology&sort=price_desc"  # first: Domain-Driven Design, 1450.00
curl "http://localhost:5080/api/v1/catalog/books?pageSize=49"                   # 400 problem+json
curl "http://localhost:5080/api/v1/catalog/books/99999"                         # 404 problem+json
curl "http://localhost:5080/api/getBooks"                                       # 404 — legacy route gone
```

## 4. Run the web app

```powershell
npm --prefix web install
npm --prefix web run dev      # http://localhost:5173
```

Check: 12 cards, "30 books", "Page 1 of 3"; search `le guin`; genre Technology + Price high→low; open Dune.

## 5. Run the suites (both must pass — Constitution V)

```powershell
dotnet test
npx --prefix web playwright install chromium   # first time only
npm --prefix web run test:e2e
```
