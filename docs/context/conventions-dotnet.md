# .NET conventions

- Target `net10.0`, `<Nullable>enable</Nullable>`, file-scoped namespaces `BookStore.Api.Features.<Feature>`.
- **Controllers are thin.** `[ApiController]`, `[Route("api/v1/<feature>")]`, inject `I<Feature>Service`,
  never `BookStoreDbContext`. No business logic, no LINQ in controllers.
- **Services own the rules.** Async all the way (`Task<T>`, pass `CancellationToken`). Read queries use
  `AsNoTracking()`.
- **DTOs, not entities.** Responses are `record` types in `<Feature>Contracts.cs`. Never return an EF entity.
- **Errors are ProblemDetails (RFC 9457).** `404` → `Problem(statusCode: 404, title: ...)`;
  validation → `ValidationProblem(...)`; business conflicts → `409`.
- **Money is `decimal`.** Column type `numeric(10,2)`. Never `double` or `float` for prices, totals or amounts.
- **Data access is LINQ.** `FromSqlRaw`/`ExecuteSqlRaw` with interpolated or concatenated strings are forbidden.
- Naming: PascalCase public members, `_camelCase` private fields, `Async` suffix on async methods.
- Logging via `ILogger<T>` with message templates (`"Book {BookId} not found"`), never `Console.WriteLine`.
