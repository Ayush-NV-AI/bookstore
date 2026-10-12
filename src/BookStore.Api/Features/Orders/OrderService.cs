using BookStore.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Orders;

public interface IOrderService
{
    Task<OrderView> PlaceAsync(Guid cartId, string customerEmail, CancellationToken cancellationToken);
    Task<OrderView> GetAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderView>> ListByEmailAsync(string customerEmail, CancellationToken cancellationToken);
}

public sealed class OrderService(BookStoreDbContext db) : IOrderService
{
    public async Task<OrderView> PlaceAsync(Guid cartId, string customerEmail, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken)
            ?? throw new NotFoundException($"No cart with id {cartId} exists.");

        if (cart.Items.Count == 0)
        {
            throw new DomainValidationException("Cart is empty.");
        }

        // Stock is re-checked at checkout: another shopper may have bought the last copies (spec US2-4).
        foreach (var item in cart.Items)
        {
            if (item.Quantity > item.Book.StockQuantity)
            {
                throw new ConflictException(
                    $"Only {item.Book.StockQuantity} copies of '{item.Book.Title}' are in stock.");
            }
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = NewOrderNumber(),
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            Status = OrderStatus.Placed,
            PlacedAt = DateTimeOffset.UtcNow,
        };

        foreach (var item in cart.Items.OrderBy(i => i.Book.Title))
        {
            // Snapshot title and price (research R3); decimal arithmetic only (Constitution IV).
            order.Lines.Add(new OrderLine
            {
                BookId = item.BookId,
                Title = item.Book.Title,
                UnitPrice = item.Book.Price,
                Quantity = item.Quantity,
                LineTotal = item.Book.Price * item.Quantity,
            });
            item.Book.StockQuantity -= item.Quantity;
        }

        order.Total = order.Lines.Sum(l => l.LineTotal);

        db.Orders.Add(order);
        db.Carts.Remove(cart);
        await db.SaveChangesAsync(cancellationToken); // one atomic save (research R4)

        return ToView(order);
    }

    public async Task<OrderView> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new NotFoundException($"No order with id {orderId} exists.");

        return ToView(order);
    }

    public async Task<IReadOnlyList<OrderView>> ListByEmailAsync(string customerEmail, CancellationToken cancellationToken)
    {
        var email = customerEmail.Trim().ToLowerInvariant();
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.CustomerEmail == email)
            .ToListAsync(cancellationToken);

        // Ordered in memory: DateTimeOffset ordering is not translated by every provider.
        return orders.OrderByDescending(o => o.PlacedAt).Select(ToView).ToList();
    }

    private static string NewOrderNumber() =>
        $"BS-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(0, 1_000_000):D6}";

    private static OrderView ToView(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Status.ToString(),
            order.CustomerEmail,
            order.PlacedAt,
            order.Total,
            order.Lines
                .OrderBy(l => l.Title)
                .Select(l => new OrderLineView(l.BookId, l.Title, l.UnitPrice, l.Quantity, l.LineTotal))
                .ToList());
}
