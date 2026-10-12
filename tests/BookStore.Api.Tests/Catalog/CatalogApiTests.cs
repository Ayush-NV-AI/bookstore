using System.Net;
using System.Net.Http.Json;
using BookStore.Api.Features.Catalog;

namespace BookStore.Api.Tests.Catalog;

/// <summary>Acceptance scenarios from specs/001-book-catalog/spec.md, one test per scenario or edge case.</summary>
public sealed class CatalogApiTests(BookStoreApiFactory factory) : IClassFixture<BookStoreApiFactory>
{
    private const string BooksUrl = "/api/v1/catalog/books";
    private readonly HttpClient _client = factory.CreateClient();

    private Task<PagedResult<BookSummary>?> ListAsync(string query = "") =>
        _client.GetFromJsonAsync<PagedResult<BookSummary>>(BooksUrl + query);

    // US1 — Browse

    [Fact]
    public async Task ListBooks_Default_ReturnsFirstPageOfTwelveSortedByTitle()
    {
        var result = await ListAsync();

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(12, result.PageSize);
        Assert.Equal(30, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(12, result.Items.Count);
        Assert.Equal("A Wizard of Earthsea", result.Items[0].Title);
        var titles = result.Items.Select(b => b.Title).ToList();
        Assert.Equal(titles.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList(), titles);
    }

    [Fact]
    public async Task ListBooks_PageThree_ReturnsRemainingSix()
    {
        var result = await ListAsync("?page=3");

        Assert.NotNull(result);
        Assert.Equal(3, result.Page);
        Assert.Equal(6, result.Items.Count);
    }

    [Fact]
    public async Task ListBooks_PastLastPage_ReturnsEmptyItemsWithCorrectTotals()
    {
        var result = await ListAsync("?page=9");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(30, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Theory]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=49")]
    [InlineData("?page=0")]
    [InlineData("?sort=cheapest")]
    public async Task ListBooks_InvalidQuery_Returns400ProblemDetails(string query)
    {
        var response = await _client.GetAsync(BooksUrl + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ListBooks_OutOfStockBook_IsListedAndFlagged()
    {
        var result = await ListAsync("?search=dragon%20tattoo");

        var book = Assert.Single(result!.Items);
        Assert.False(book.InStock);
        Assert.Equal(525.00m, book.Price);
    }

    [Fact]
    public async Task ListBooks_Prices_AreStoredListPricesWithNoDiscount()
    {
        // Legacy code took 10% off Fiction and 5% off anything over Rs.500 (FR-009, FR-012).
        var fiction = await ListAsync("?search=pride%20and%20prejudice");
        var expensive = await ListAsync("?search=domain-driven");

        Assert.Equal(250.00m, Assert.Single(fiction!.Items).Price);
        Assert.Equal(1450.00m, Assert.Single(expensive!.Items).Price);
    }

    // US2 — Search, filter, sort

    [Fact]
    public async Task ListBooks_SearchByAuthorAnyCase_ReturnsOnlyMatches()
    {
        var result = await ListAsync("?search=LE%20GUIN");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, b => Assert.Equal("Ursula K. Le Guin", b.Author));
    }

    [Fact]
    public async Task ListBooks_SearchIsTrimmed()
    {
        var result = await ListAsync("?search=%20%20dune%20%20");

        Assert.Equal("Dune", Assert.Single(result!.Items).Title);
    }

    [Fact]
    public async Task ListBooks_SearchWithSqlMetacharacters_IsPlainTextAndReturnsNothing()
    {
        var response = await _client.GetAsync(BooksUrl + "?search=%27%20OR%201%3D1%20--");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookSummary>>();
        Assert.Equal(0, result!.TotalCount);
    }

    [Fact]
    public async Task ListBooks_SearchOver100Characters_Returns400()
    {
        var response = await _client.GetAsync(BooksUrl + "?search=" + new string('a', 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListBooks_GenreTechnologySortedByPriceDesc_MostExpensiveFirst()
    {
        var result = await ListAsync("?genre=technology&sort=price_desc");

        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal("Domain-Driven Design", result.Items[0].Title);
        Assert.Equal(result.Items.Select(b => b.Price).OrderByDescending(p => p), result.Items.Select(b => b.Price));
    }

    [Fact]
    public async Task ListBooks_UnknownGenre_ReturnsEmptyPageNotError()
    {
        var result = await ListAsync("?genre=Poetry");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task ListGenres_ReturnsEightDistinctGenresAlphabetically()
    {
        var genres = await _client.GetFromJsonAsync<List<string>>("/api/v1/catalog/genres");

        Assert.NotNull(genres);
        Assert.Equal(8, genres.Count);
        Assert.Equal(genres.OrderBy(g => g, StringComparer.Ordinal), genres);
    }

    // US3 — Detail

    [Fact]
    public async Task GetBook_Existing_ReturnsFullDetail()
    {
        var page = await ListAsync("?search=dune");
        var id = Assert.Single(page!.Items).Id;

        var book = await _client.GetFromJsonAsync<BookDetail>($"{BooksUrl}/{id}");

        Assert.NotNull(book);
        Assert.Equal("9780001000001", book.Isbn);
        Assert.Equal(599.00m, book.Price);
        Assert.Equal(1965, book.PublishedYear);
        Assert.True(book.InStock);
        Assert.False(string.IsNullOrWhiteSpace(book.Description));
    }

    [Fact]
    public async Task GetBook_Unknown_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"{BooksUrl}/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    // FR-012 / FR-013

    [Theory]
    [InlineData("/api/getBooks")]
    [InlineData("/api/Book/1")]
    [InlineData("/api/searchBooks?q=dune")]
    [InlineData("/api/books/cheap")]
    [InlineData("/api/books/category/Fiction")]
    public async Task LegacyEndpoints_AreRemoved(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_WhenDatabaseReachable_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
