using System.Net;
using System.Net.Http.Json;
using BookStore.Api.Features.Carts;
using BookStore.Api.Features.Catalog;
using BookStore.Api.Features.Orders;

namespace BookStore.Api.Tests.Orders;

/// <summary>Spec 002 US2 and US3 acceptance scenarios.</summary>
public sealed class OrdersApiTests(BookStoreApiFactory factory) : IClassFixture<BookStoreApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private Task<HttpResponseMessage> PlaceAsync(Guid cartId, string email) =>
        _client.PostAsJsonAsync("/api/v1/orders", new { cartId, customerEmail = email });

    private async Task<Guid> CartWithAsync(string title, int quantity)
    {
        var cart = await _client.CreateCartAsync();
        var book = await _client.FindBookAsync(title);
        (await _client.SetQuantityAsync(cart.Id, book.Id, quantity)).EnsureSuccessStatusCode();
        return cart.Id;
    }

    private async Task<BookDetail> GetBookAsync(string title)
    {
        var summary = await _client.FindBookAsync(title);
        return (await _client.GetFromJsonAsync<BookDetail>($"/api/v1/catalog/books/{summary.Id}"))!;
    }

    [Fact]
    public async Task PlaceOrder_ValidCart_CreatesPlacedOrderSnapshotsPricesReducesStockAndDeletesCart()
    {
        var stockBefore = (await GetBookAsync("Atomic Habits")).StockQuantity;
        var cartId = await CartWithAsync("Atomic Habits", 2);

        var response = await PlaceAsync(cartId, "  Asha@Example.com ");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var order = await response.Content.ReadFromJsonAsync<OrderView>();
        Assert.NotNull(order);
        Assert.Equal("Placed", order.Status);
        Assert.Equal("asha@example.com", order.CustomerEmail);
        Assert.Matches(@"^BS-\d{8}-\d{6}$", order.OrderNumber);
        var line = Assert.Single(order.Lines);
        Assert.Equal("Atomic Habits", line.Title);
        Assert.Equal(549.00m, line.UnitPrice);
        Assert.Equal(1098.00m, line.LineTotal);
        Assert.Equal(1098.00m, order.Total);

        Assert.Equal(stockBefore - 2, (await GetBookAsync("Atomic Habits")).StockQuantity);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/v1/carts/{cartId}")).StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_TotalAlwaysEqualsSumOfLineTotals()
    {
        var cart = await _client.CreateCartAsync();
        foreach (var (title, qty) in new[] { ("Clean Code", 1), ("Refactoring", 2), ("Sapiens", 3) })
        {
            var book = await _client.FindBookAsync(title);
            (await _client.SetQuantityAsync(cart.Id, book.Id, qty)).EnsureSuccessStatusCode();
        }

        var order = await (await PlaceAsync(cart.Id, "sum@example.com")).Content.ReadFromJsonAsync<OrderView>();

        Assert.Equal(899.00m + 2 * 1099.00m + 3 * 599.00m, order!.Total);
        Assert.Equal(order.Lines.Sum(l => l.LineTotal), order.Total);
    }

    [Fact]
    public async Task PlaceOrder_EmptyCart_Returns400()
    {
        var cart = await _client.CreateCartAsync();

        var response = await PlaceAsync(cart.Id, "empty@example.com");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Cart is empty", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task PlaceOrder_InvalidEmail_Returns400()
    {
        var cartId = await CartWithAsync("Dune", 1);

        var response = await PlaceAsync(cartId, "not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_UnknownCart_Returns404()
    {
        var response = await PlaceAsync(Guid.NewGuid(), "ghost@example.com");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_LastCopiesBoughtByAnotherShopper_Returns409AndKeepsCart()
    {
        // Domain-Driven Design has 3 copies. Two shoppers each put all 3 in their carts.
        var first = await CartWithAsync("Domain-Driven Design", 3);
        var second = await CartWithAsync("Domain-Driven Design", 3);

        var winner = await PlaceAsync(first, "first@example.com");
        var loser = await PlaceAsync(second, "second@example.com");

        Assert.Equal(HttpStatusCode.Created, winner.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, loser.StatusCode);
        Assert.Contains("Domain-Driven Design", await loser.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/v1/carts/{second}")).StatusCode);
    }

    [Fact]
    public async Task GetOrder_Placed_ReturnsOrder()
    {
        var placed = await (await PlaceAsync(await CartWithAsync("Dune", 1), "get@example.com"))
            .Content.ReadFromJsonAsync<OrderView>();

        var order = await _client.GetFromJsonAsync<OrderView>($"/api/v1/orders/{placed!.Id}");

        Assert.Equal(placed.OrderNumber, order!.OrderNumber);
        Assert.Equal(599.00m, order.Total);
    }

    [Fact]
    public async Task GetOrder_Unknown_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ListOrders_ByEmail_ReturnsNewestFirst()
    {
        var older = await (await PlaceAsync(await CartWithAsync("Foundation", 1), "list@example.com"))
            .Content.ReadFromJsonAsync<OrderView>();
        await Task.Delay(20);
        var newer = await (await PlaceAsync(await CartWithAsync("The Hobbit", 1), "LIST@example.com"))
            .Content.ReadFromJsonAsync<OrderView>();

        var orders = await _client.GetFromJsonAsync<List<OrderView>>("/api/v1/orders?customerEmail=list@example.com");

        Assert.NotNull(orders);
        Assert.Equal(2, orders.Count);
        Assert.Equal(newer!.Id, orders[0].Id);
        Assert.Equal(older!.Id, orders[1].Id);
    }

    [Fact]
    public async Task ListOrders_MissingEmail_Returns400()
    {
        var response = await _client.GetAsync("/api/v1/orders");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
