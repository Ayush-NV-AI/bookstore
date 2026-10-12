# Quickstart: Orders and Cart

## Local (database in Docker, API + web on the host)

```powershell
docker compose down -v          # new tables — reset the schema
docker compose up -d db
dotnet test                     # Catalog + Cart + Orders
npm --prefix web run test:e2e   # catalog.spec.ts + checkout.spec.ts
```

## Contract smoke test (PowerShell)

```powershell
$bookId = (Invoke-RestMethod "http://localhost:5080/api/v1/catalog/books?search=pride").items[0].id
$cart = Invoke-RestMethod -Method Post http://localhost:5080/api/v1/carts
Invoke-RestMethod -Method Put "http://localhost:5080/api/v1/carts/$($cart.id)/items/$bookId" -ContentType 'application/json' -Body '{"quantity":2}'
$order = Invoke-RestMethod -Method Post http://localhost:5080/api/v1/orders -ContentType 'application/json' -Body (@{cartId=$cart.id; customerEmail='asha@example.com'} | ConvertTo-Json)
$order.status   # Placed
$order.total    # 500.00 (Pride and Prejudice, 250.00 × 2)
```

## Full stack in containers

```powershell
docker compose --profile full up -d --build
# web: http://localhost:8080   api: http://localhost:5080
$env:E2E_BASE_URL = 'http://localhost:8080'; npm --prefix web run test:e2e; Remove-Item Env:E2E_BASE_URL
```

## MCP (read-only database access for the agent)

```powershell
$env:BOOKSTORE_RO_PASSWORD = 'bookstore_ro_local'   # local-only role created by db/init/01-readonly-role.sql
gemini
/mcp            # bookstore-db: list_tables, execute_sql
```
