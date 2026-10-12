using System.Net.Http.Json;
using BookStore.Api.Features.Carts;
using BookStore.Api.Features.Catalog;

namespace BookStore.Api.Tests;

/// <summary>Small helpers shared by API tests. Book ids are looked up by title, never hard-coded.</summary>
public static class ApiTestHelpers
{
    public static async Task<BookSummary> FindBookAsync(this HttpClient client, string title)
    {
        var page = await client.GetFromJsonAsync<PagedResult<BookSummary>>(
            $"/api/v1/catalog/books?search={Uri.EscapeDataString(title)}");
        return page!.Items.Single(b => b.Title == title);
    }

    public static async Task<CartView> CreateCartAsync(this HttpClient client)
    {
        var response = await client.PostAsync("/api/v1/carts", content: null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartView>())!;
    }

    public static Task<HttpResponseMessage> SetQuantityAsync(this HttpClient client, Guid cartId, int bookId, int quantity) =>
        client.PutAsJsonAsync($"/api/v1/carts/{cartId}/items/{bookId}", new { quantity });
}
