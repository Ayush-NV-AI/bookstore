# Testing

- **Backend:** xUnit in `tests/BookStore.Api.Tests`. API tests use `WebApplicationFactory<Program>` with the
  EF Core InMemory provider and the standard seed. One test class per controller, method names
  `Method_Scenario_ExpectedResult`.
- **UI:** Playwright in `web/e2e`, Chromium only, run against the real API and database
  (`docker compose up -d db` first). Select elements by `data-testid`, never by CSS structure or text that
  may change.
- A task is **done** only when `dotnet test` and `npm --prefix web run test:e2e` both pass. Never mark a
  task `[X]` in `tasks.md` on a red suite, never delete or skip a failing test to get green.
- Write the test from the acceptance scenario in `spec.md` first; name it after the scenario.
