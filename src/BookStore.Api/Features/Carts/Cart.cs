using BookStore.Api.Features.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Api.Features.Carts;

/// <summary>An anonymous shopper's basket (specs/002-orders-cart/data-model.md).</summary>
public class Cart
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<CartItem> Items { get; set; } = [];
}

public class CartItem
{
    public const int MaxQuantity = 10;

    public int Id { get; set; }
    public Guid CartId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public Book Book { get; set; } = null!;
}

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> cart)
    {
        cart.ToTable("Carts");
        cart.HasKey(c => c.Id);
        cart.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> item)
    {
        item.ToTable("CartItems");
        item.HasKey(i => i.Id);
        item.HasIndex(i => new { i.CartId, i.BookId }).IsUnique();
        item.HasOne(i => i.Book).WithMany().HasForeignKey(i => i.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
