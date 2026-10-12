using BookStore.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Catalog;

public interface ICatalogService
{
    Task<PagedResult<BookSummary>> ListBooksAsync(CatalogQuery query, CancellationToken cancellationToken);
    Task<BookDetail?> GetBookAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ListGenresAsync(CancellationToken cancellationToken);
}

public sealed class CatalogService(BookStoreDbContext db) : ICatalogService
{
    public async Task<PagedResult<BookSummary>> ListBooksAsync(CatalogQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Book> books = db.Books.AsNoTracking();

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            // research.md R3: parameterised lower(..) LIKE on PostgreSQL, portable to the InMemory test provider.
            var term = search.ToLower();
            books = books.Where(b => b.Title.ToLower().Contains(term) || b.Author.ToLower().Contains(term));
        }

        var genre = query.Genre?.Trim();
        if (!string.IsNullOrEmpty(genre))
        {
            var genreKey = genre.ToLower();
            books = books.Where(b => b.Genre.ToLower() == genreKey);
        }

        var totalCount = await books.CountAsync(cancellationToken);

        var ordered = query.Sort switch
        {
            CatalogSort.PriceAsc => books.OrderBy(b => b.Price).ThenBy(b => b.Title),
            CatalogSort.PriceDesc => books.OrderByDescending(b => b.Price).ThenBy(b => b.Title),
            CatalogSort.Newest => books.OrderByDescending(b => b.PublishedYear).ThenBy(b => b.Title),
            _ => books.OrderBy(b => b.Title),
        };

        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BookSummary(b.Id, b.Title, b.Author, b.Genre, b.Price, b.StockQuantity > 0))
            .ToListAsync(cancellationToken);

        var totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

        return new PagedResult<BookSummary>(items, query.Page, query.PageSize, totalCount, totalPages);
    }

    public async Task<BookDetail?> GetBookAsync(int id, CancellationToken cancellationToken) =>
        await db.Books
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookDetail(
                b.Id, b.Isbn, b.Title, b.Author, b.Genre, b.Price,
                b.PublishedYear, b.StockQuantity, b.StockQuantity > 0, b.Description))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ListGenresAsync(CancellationToken cancellationToken) =>
        await db.Books
            .AsNoTracking()
            .Select(b => b.Genre)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(cancellationToken);
}
