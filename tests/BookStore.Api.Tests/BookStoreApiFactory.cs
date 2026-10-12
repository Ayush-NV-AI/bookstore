using BookStore.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Api.Tests;

/// <summary>
/// Hosts the real API in-process on the EF Core InMemory provider with the standard seed.
/// Each factory instance (one per test class) gets its own database, so classes cannot interfere.
/// </summary>
public sealed class BookStoreApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"bookstore-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
            services.AddDbContext<BookStoreDbContext>(options => options.UseInMemoryDatabase(_databaseName)));
    }
}
