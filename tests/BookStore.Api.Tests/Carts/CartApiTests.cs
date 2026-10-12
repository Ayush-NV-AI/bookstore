using System.Net;
using System.Net.Http.Json;
using BookStore.Api.Features.Carts;

namespace BookStore.Api.Tests.Carts;

/// <summary>Spec 002 US1 acceptance scenarios and cart edge cases.</summary>
public sealed class CartApiTests(BookStoreApiFactory factory) : IClassFixture<BookStoreApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateCart_Returns201WithLocationAndEmptyCart()
    {
        var response = await _client.PostAsync("/api/v1/carts", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var cart = await response.Content.ReadFromJsonAsync<CartView>();
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.ItemCount);
        Assert.Equal(0m, cart.Subtotal);
    }

    [Fact]
    public async Task SetQuantity_NewLine_ServerComputesLineTotalAndSubtotal()
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("Dune");

        var response = await _client.SetQuantityAsync(cart.Id, book.Id, 2);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CartView>();
        var line = Assert.Single(updated!.Items);
        Assert.Equal(599.00m, line.UnitPrice);
        Assert.Equal(1198.00m, line.LineTotal);
        Assert.Equal(2, updated.ItemCount);
        Assert.Equal(1198.00m, updated.Subtotal);
    }

    [Fact]
    public async Task SetQuantity_SameBookTwice_ReplacesQuantityInsteadOfAdding()
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("Foundation");

        await _client.SetQuantityAsync(cart.Id, book.Id, 3);
        var response = await _client.SetQuantityAsync(cart.Id, book.Id, 1);

        var updated = await response.Content.ReadFromJsonAsync<CartView>();
        Assert.Equal(1, Assert.Single(updated!.Items).Quantity);
    }

    [Fact]
    public async Task SetQuantity_TwoBooks_SubtotalIsSumOfLineTotals()
    {
        var cart = await _client.CreateCartAsync();
        var dune = await _client.FindBookAsync("Dune");
        var hobbit = await _client.FindBookAsync("The Hobbit");

        await _client.SetQuantityAsync(cart.Id, dune.Id, 1);
        var response = await _client.SetQuantityAsync(cart.Id, hobbit.Id, 3);

        var updated = await response.Content.ReadFromJsonAsync<CartView>();
        Assert.Equal(2, updated!.Items.Count);
        Assert.Equal(4, updated.ItemCount);
        Assert.Equal(599.00m + 3 * 350.00m, updated.Subtotal);
    }

    [Fact]
    public async Task SetQuantity_AboveStock_Returns409WithStockInMessage()
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("Neuromancer"); // 4 in stock

        var response = await _client.SetQuantityAsync(cart.Id, book.Id, 5);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Only 4 copies", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SetQuantity_OutOfStockBook_Returns409()
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("The Girl with the Dragon Tattoo");

        var response = await _client.SetQuantityAsync(cart.Id, book.Id, 1);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task SetQuantity_OutOfRange_Returns400(int quantity)
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("Dune");

        var response = await _client.SetQuantityAsync(cart.Id, book.Id, quantity);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetQuantity_UnknownBook_Returns404()
    {
        var cart = await _client.CreateCartAsync();

        var response = await _client.SetQuantityAsync(cart.Id, 999_999, 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_Unknown_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/v1/carts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RemoveItem_RemovesLine_AndIsIdempotent()
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync("Dune");
        await _client.SetQuantityAsync(cart.Id, book.Id, 1);

        var first = await _client.DeleteAsync($"/api/v1/carts/{cart.Id}/items/{book.Id}");
        var second = await _client.DeleteAsync($"/api/v1/carts/{cart.Id}/items/{book.Id}");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var updated = await second.Content.ReadFromJsonAsync<CartView>();
        Assert.Empty(updated!.Items);
        Assert.Equal(0m, updated.Subtotal);
    }
}
