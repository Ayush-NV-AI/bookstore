using System.ComponentModel.DataAnnotations;

namespace BookStore.Api.Features.Carts;

public sealed record CartLineView(int BookId, string Title, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record CartView(Guid Id, IReadOnlyList<CartLineView> Items, int ItemCount, decimal Subtotal);

public sealed class SetQuantityRequest
{
    [Range(1, CartItem.MaxQuantity)]
    public int Quantity { get; set; }
}
