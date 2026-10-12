using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.Api.Features.Catalog;

/// <summary>A title for sale. See specs/001-book-catalog/data-model.md.</summary>
public class Book
{
    public int Id { get; set; }
    public required string Isbn { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Genre { get; set; }

    /// <summary>INR list price. Money is decimal (Constitution IV).</summary>
    public decimal Price { get; set; }

    public int PublishedYear { get; set; }
    public int StockQuantity { get; set; }
    public string Description { get; set; } = string.Empty;
}

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> book)
    {
        book.ToTable("Books");
        book.HasKey(b => b.Id);
        book.Property(b => b.Isbn).HasMaxLength(13).IsRequired();
        book.HasIndex(b => b.Isbn).IsUnique();
        book.Property(b => b.Title).HasMaxLength(200).IsRequired();
        book.Property(b => b.Author).HasMaxLength(150).IsRequired();
        book.Property(b => b.Genre).HasMaxLength(50).IsRequired();
        book.HasIndex(b => b.Genre);
        book.Property(b => b.Price).HasPrecision(10, 2);
        book.Property(b => b.Description).HasMaxLength(2000);
    }
}
