# API directory rules (loaded just-in-time when a tool touches src/BookStore.Api)

- New code goes under `Features/<Feature>/` or `Infrastructure/`. Never add files to `Controllers/`,
  `Utils/` or `Models/` — those folders are legacy and will be deleted.
- Register every new service in `Program.cs` as `AddScoped<IXService, XService>()`.
- After any change in this directory run `dotnet build` and fix warnings you introduced.
- The DbContext is `BookStoreDbContext` (target) — the legacy `AppDb` is removed by spec 001.
