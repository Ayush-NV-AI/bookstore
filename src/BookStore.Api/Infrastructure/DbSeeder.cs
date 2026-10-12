using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Infrastructure;

public static class DbSeeder
{
    /// <summary>
    /// Creates the schema (training simplification — no migrations, see research.md R9) and seeds the
    /// catalog once. A schema change needs `docker compose down -v`.
    /// </summary>
    public static async Task InitialiseAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();

        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (!await db.Books.AnyAsync(cancellationToken))
        {
            db.Books.AddRange(SeedBooks.Create());
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
