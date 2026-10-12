using System.ComponentModel.DataAnnotations;

namespace BookStore.Api.Features.Catalog;

public sealed record BookSummary(int Id, string Title, string Author, string Genre, decimal Price, bool InStock);

public sealed record BookDetail(
    int Id,
    string Isbn,
    string Title,
    string Author,
    string Genre,
    decimal Price,
    int PublishedYear,
    int StockQuantity,
    bool InStock,
    string Description);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public static class CatalogSort
{
    public const string Title = "title";
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string Newest = "newest";

    public static readonly IReadOnlyList<string> All = [Title, PriceAsc, PriceDesc, Newest];
}

/// <summary>Query string for GET /api/v1/catalog/books (FR-002 … FR-006).</summary>
public sealed class CatalogQuery : IValidatableObject
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 48;

    [StringLength(100)]
    public string? Search { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    /// <summary>One of <see cref="CatalogSort.All"/>; null or omitted means <see cref="CatalogSort.Title"/>.</summary>
    public string? Sort { get; set; }

    [Range(1, 100_000)]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = DefaultPageSize;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Sort is not null && !CatalogSort.All.Contains(Sort))
        {
            yield return new ValidationResult(
                $"Sort must be one of: {string.Join(", ", CatalogSort.All)}.",
                [nameof(Sort)]);
        }
    }
}
