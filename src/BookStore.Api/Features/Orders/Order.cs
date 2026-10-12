using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Api.Features.Orders;

public enum OrderStatus
{
    Placed,
}

/// <summary>A placed purchase. Lines are snapshots and never change (specs/002-orders-cart/research.md R3).</summary>
public class Order
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerEmail { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public DateTimeOffset PlacedAt { get; set; }
    public decimal Total { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderLine
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public int BookId { get; set; }
    public required string Title { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> order)
    {
        order.ToTable("Orders");
        order.HasKey(o => o.Id);
        order.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
        order.HasIndex(o => o.OrderNumber).IsUnique();
        order.Property(o => o.CustomerEmail).HasMaxLength(254).IsRequired();
        order.HasIndex(o => o.CustomerEmail);
        order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        order.Property(o => o.Total).HasPrecision(10, 2);
        order.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> line)
    {
        line.ToTable("OrderLines");
        line.HasKey(l => l.Id);
        line.Property(l => l.Title).HasMaxLength(200).IsRequired();
        line.Property(l => l.UnitPrice).HasPrecision(10, 2);
        line.Property(l => l.LineTotal).HasPrecision(10, 2);
    }
}
