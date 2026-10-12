using BookStore.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Carts;

public interface ICartService
{
    Task<CartView> CreateAsync(CancellationToken cancellationToken);
    Task<CartView> GetAsync(Guid cartId, CancellationToken cancellationToken);
    Task<CartView> SetQuantityAsync(Guid cartId, int bookId, int quantity, CancellationToken cancellationToken);
    Task<CartView> RemoveAsync(Guid cartId, int bookId, CancellationToken cancellationToken);
}

public sealed class CartService(BookStoreDbContext db) : ICartService
{
    public async Task<CartView> CreateAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var cart = new Cart { Id = Guid.NewGuid(), CreatedAt = now, UpdatedAt = now };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(cancellationToken);
        return ToView(cart);
    }

    public async Task<CartView> GetAsync(Guid cartId, CancellationToken cancellationToken) =>
        ToView(await LoadAsync(cartId, cancellationToken));

    public async Task<CartView> SetQuantityAsync(Guid cartId, int bookId, int quantity, CancellationToken cancellationToken)
    {
        var cart = await LoadAsync(cartId, cancellationToken);
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException($"No book with id {bookId} exists.");

        if (quantity > book.StockQuantity)
        {
            throw new ConflictException($"Only {book.StockQuantity} copies of '{book.Title}' are in stock.");
        }

        var line = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (line is null)
        {
            cart.Items.Add(new CartItem { CartId = cart.Id, BookId = bookId, Quantity = quantity, Book = book });
        }
        else
        {
            line.Quantity = quantity; // set, not add (spec edge case)
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return ToView(cart);
    }

    public async Task<CartView> RemoveAsync(Guid cartId, int bookId, CancellationToken cancellationToken)
    {
        var cart = await LoadAsync(cartId, cancellationToken);
        var line = cart.Items.FirstOrDefault(i => i.BookId == bookId);
        if (line is not null)
        {
            cart.Items.Remove(line);
            db.CartItems.Remove(line);
            cart.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return ToView(cart); // idempotent: removing an absent line is not an error
    }

    private async Task<Cart> LoadAsync(Guid cartId, CancellationToken cancellationToken) =>
        await db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken)
        ?? throw new NotFoundException($"No cart with id {cartId} exists.");

    private static CartView ToView(Cart cart)
    {
        // Money is computed here, on the server, in decimal (Constitution IV).
        var lines = cart.Items
            .OrderBy(i => i.Book.Title)
            .Select(i => new CartLineView(i.BookId, i.Book.Title, i.Book.Price, i.Quantity, i.Book.Price * i.Quantity))
            .ToList();

        return new CartView(cart.Id, lines, lines.Sum(l => l.Quantity), lines.Sum(l => l.LineTotal));
    }
}
