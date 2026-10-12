using BookStore.Api.Features.Catalog;

namespace BookStore.Api.Infrastructure;

/// <summary>30 books across 8 genres. Prices in INR. "The Girl with the Dragon Tattoo" is deliberately out of stock.</summary>
public static class SeedBooks
{
    public static IReadOnlyList<Book> Create() =>
    [
        Make("9780001000001", "Dune", "Frank Herbert", "Science Fiction", 599.00m, 1965, 14,
            "A desert planet, a noble family and the spice that controls the universe."),
        Make("9780001079199", "Foundation", "Isaac Asimov", "Science Fiction", 449.00m, 1951, 9,
            "A mathematician predicts the fall of an empire and plans the recovery."),
        Make("9780001158382", "The Left Hand of Darkness", "Ursula K. Le Guin", "Science Fiction", 399.00m, 1969, 6,
            "An envoy on a winter world where gender is fluid."),
        Make("9780001237575", "Neuromancer", "William Gibson", "Science Fiction", 425.50m, 1984, 4,
            "A washed-up hacker is hired for one last run in cyberspace."),
        Make("9780001316768", "Project Hail Mary", "Andy Weir", "Science Fiction", 699.00m, 2021, 12,
            "A lone astronaut wakes up with no memory and a mission to save Earth."),
        Make("9780001395954", "The Hobbit", "J.R.R. Tolkien", "Fantasy", 350.00m, 1937, 20,
            "A reluctant hobbit joins a company of dwarves on a quest for treasure."),
        Make("9780001475144", "A Wizard of Earthsea", "Ursula K. Le Guin", "Fantasy", 320.00m, 1968, 7,
            "A young mage unleashes a shadow and must hunt it across the sea."),
        Make("9780001554337", "The Name of the Wind", "Patrick Rothfuss", "Fantasy", 650.00m, 2007, 5,
            "A legendary figure tells the true story of his life."),
        Make("9780001633520", "Mistborn: The Final Empire", "Brandon Sanderson", "Fantasy", 575.00m, 2006, 11,
            "A street thief discovers she can burn metals for power."),
        Make("9780001712713", "Pride and Prejudice", "Jane Austen", "Fiction", 250.00m, 1813, 25,
            "Elizabeth Bennet and Mr Darcy misjudge each other, then don't."),
        Make("9780001791909", "To Kill a Mockingbird", "Harper Lee", "Fiction", 399.00m, 1960, 15,
            "A lawyer defends a Black man in a small Alabama town."),
        Make("9780001871090", "The Great Gatsby", "F. Scott Fitzgerald", "Fiction", 299.00m, 1925, 18,
            "A mysterious millionaire chases a lost love on Long Island."),
        Make("9780001950283", "One Hundred Years of Solitude", "Gabriel Garcia Marquez", "Fiction", 499.00m, 1967, 8,
            "Seven generations of the Buendia family in Macondo."),
        Make("9780002029476", "The God of Small Things", "Arundhati Roy", "Fiction", 450.00m, 1997, 10,
            "Twins in Kerala and the small things that shape their lives."),
        Make("9780002108669", "The Murder of Roger Ackroyd", "Agatha Christie", "Mystery", 275.00m, 1926, 13,
            "Hercule Poirot investigates a village murder with a famous twist."),
        Make("9780002187855", "The Hound of the Baskervilles", "Arthur Conan Doyle", "Mystery", 225.00m, 1902, 16,
            "Sherlock Holmes and a spectral hound on the moors."),
        Make("9780002267045", "Gone Girl", "Gillian Flynn", "Mystery", 499.00m, 2012, 9,
            "A wife disappears and the marriage unravels in public."),
        Make("9780002346238", "The Girl with the Dragon Tattoo", "Stieg Larsson", "Mystery", 525.00m, 2005, 0,
            "A journalist and a hacker investigate a decades-old disappearance."),
        Make("9780002425421", "Clean Code", "Robert C. Martin", "Technology", 899.00m, 2008, 7,
            "Principles and practices for writing readable, maintainable code."),
        Make("9780002504614", "The Pragmatic Programmer", "Andrew Hunt and David Thomas", "Technology", 949.00m, 1999, 6,
            "Pragmatic advice for software craftspeople."),
        Make("9780002583800", "Designing Data-Intensive Applications", "Martin Kleppmann", "Technology", 1299.00m, 2017, 5,
            "The big ideas behind reliable, scalable, maintainable systems."),
        Make("9780002662994", "Domain-Driven Design", "Eric Evans", "Technology", 1450.00m, 2003, 3,
            "Tackling complexity in the heart of software."),
        Make("9780002742184", "Refactoring", "Martin Fowler", "Technology", 1099.00m, 1999, 4,
            "Improving the design of existing code, one small step at a time."),
        Make("9780002821377", "The Lean Startup", "Eric Ries", "Business", 499.00m, 2011, 12,
            "Build, measure, learn: a method for building startups."),
        Make("9780002900560", "Good to Great", "Jim Collins", "Business", 599.00m, 2001, 8,
            "Why some companies make the leap and others don't."),
        Make("9780002979757", "Thinking, Fast and Slow", "Daniel Kahneman", "Business", 650.00m, 2011, 10,
            "The two systems that drive the way we think."),
        Make("9780003058949", "Sapiens", "Yuval Noah Harari", "History", 599.00m, 2011, 14,
            "A brief history of humankind."),
        Make("9780003138139", "Guns, Germs, and Steel", "Jared Diamond", "History", 625.00m, 1997, 6,
            "The fates of human societies."),
        Make("9780003217322", "The Discovery of India", "Jawaharlal Nehru", "History", 399.00m, 1946, 9,
            "A sweeping account of Indian history and culture."),
        Make("9780003296518", "Atomic Habits", "James Clear", "Self-Help", 549.00m, 2018, 22,
            "Tiny changes, remarkable results.")
    ];

    private static Book Make(string isbn, string title, string author, string genre, decimal price, int year, int stock, string description) =>
        new()
        {
            Isbn = isbn,
            Title = title,
            Author = author,
            Genre = genre,
            Price = price,
            PublishedYear = year,
            StockQuantity = stock,
            Description = description,
        };
}
