# API directory rules (loaded just-in-time when a tool touches src/BookStore.Api)

- New code goes under `Features/<Feature>/` or `Infrastructure/`. Never recreate `Controllers/`,
  `Utils/`, `Models/` or `Data/` — spec 001 deleted them.
- Register every new service in `Program.cs` as `AddScoped<IXService, XService>()`.
- After any change in this directory run `dotnet build` and fix warnings you introduced.
- The DbContext is `BookStoreDbContext`. Entity mapping goes in an `IEntityTypeConfiguration<T>` next to the
  entity; the context discovers it, so do not add mapping code to the context.
- The `Testing` environment (API tests) registers the InMemory provider itself — keep the `IsEnvironment("Testing")` guard in `Program.cs`.
