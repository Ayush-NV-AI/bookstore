using System.ComponentModel.DataAnnotations;

namespace BookStore.Api.Features.Orders;

public sealed record OrderLineView(int BookId, string Title, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderView(
    Guid Id,
    string OrderNumber,
    string Status,
    string CustomerEmail,
    DateTimeOffset PlacedAt,
    decimal Total,
    IReadOnlyList<OrderLineView> Lines);

public sealed class PlaceOrderRequest
{
    [Required]
    public Guid? CartId { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string? CustomerEmail { get; set; }
}

public sealed class OrderLookupQuery
{
    [Required]
    [EmailAddress]
    public string? CustomerEmail { get; set; }
}
