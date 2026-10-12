using BookStore.Api.Models;

namespace BookStore.Api.Data
{
    public static class SeedData
    {
        public static void Seed(AppDb db)
        {
            if (db.Books.Any()) return;

            db.Books.AddRange(new List<Book>
            {
                new Book { title = "Dune", Author = "Frank Herbert", ISBN = "9780001000001", Category = "Science Fiction", Price = 599.0, stock = 14, Desc = "A desert planet, a noble family and the spice that controls the universe.", Year = 1965 },
                new Book { title = "Foundation", Author = "Isaac Asimov", ISBN = "9780001079199", Category = "Science Fiction", Price = 449.0, stock = 9, Desc = "A mathematician predicts the fall of an empire and plans the recovery.", Year = 1951 },
                new Book { title = "The Left Hand of Darkness", Author = "Ursula K. Le Guin", ISBN = "9780001158382", Category = "Science Fiction", Price = 399.0, stock = 6, Desc = "An envoy on a winter world where gender is fluid.", Year = 1969 },
                new Book { title = "Neuromancer", Author = "William Gibson", ISBN = "9780001237575", Category = "Science Fiction", Price = 425.5, stock = 4, Desc = "A washed-up hacker is hired for one last run in cyberspace.", Year = 1984 },
                new Book { title = "Project Hail Mary", Author = "Andy Weir", ISBN = "9780001316768", Category = "Science Fiction", Price = 699.0, stock = 12, Desc = "A lone astronaut wakes up with no memory and a mission to save Earth.", Year = 2021 },
                new Book { title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "9780001395954", Category = "Fantasy", Price = 350.0, stock = 20, Desc = "A reluctant hobbit joins a company of dwarves on a quest for treasure.", Year = 1937 },
                new Book { title = "A Wizard of Earthsea", Author = "Ursula K. Le Guin", ISBN = "9780001475144", Category = "Fantasy", Price = 320.0, stock = 7, Desc = "A young mage unleashes a shadow and must hunt it across the sea.", Year = 1968 },
                new Book { title = "The Name of the Wind", Author = "Patrick Rothfuss", ISBN = "9780001554337", Category = "Fantasy", Price = 650.0, stock = 5, Desc = "A legendary figure tells the true story of his life.", Year = 2007 },
                new Book { title = "Mistborn: The Final Empire", Author = "Brandon Sanderson", ISBN = "9780001633520", Category = "Fantasy", Price = 575.0, stock = 11, Desc = "A street thief discovers she can burn metals for power.", Year = 2006 },
                new Book { title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "9780001712713", Category = "Fiction", Price = 250.0, stock = 25, Desc = "Elizabeth Bennet and Mr Darcy misjudge each other, then don't.", Year = 1813 },
                new Book { title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780001791909", Category = "Fiction", Price = 399.0, stock = 15, Desc = "A lawyer defends a Black man in a small Alabama town.", Year = 1960 },
                new Book { title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780001871090", Category = "Fiction", Price = 299.0, stock = 18, Desc = "A mysterious millionaire chases a lost love on Long Island.", Year = 1925 },
                new Book { title = "One Hundred Years of Solitude", Author = "Gabriel Garcia Marquez", ISBN = "9780001950283", Category = "Fiction", Price = 499.0, stock = 8, Desc = "Seven generations of the Buendia family in Macondo.", Year = 1967 },
                new Book { title = "The God of Small Things", Author = "Arundhati Roy", ISBN = "9780002029476", Category = "Fiction", Price = 450.0, stock = 10, Desc = "Twins in Kerala and the small things that shape their lives.", Year = 1997 },
                new Book { title = "The Murder of Roger Ackroyd", Author = "Agatha Christie", ISBN = "9780002108669", Category = "Mystery", Price = 275.0, stock = 13, Desc = "Hercule Poirot investigates a village murder with a famous twist.", Year = 1926 },
                new Book { title = "The Hound of the Baskervilles", Author = "Arthur Conan Doyle", ISBN = "9780002187855", Category = "Mystery", Price = 225.0, stock = 16, Desc = "Sherlock Holmes and a spectral hound on the moors.", Year = 1902 },
                new Book { title = "Gone Girl", Author = "Gillian Flynn", ISBN = "9780002267045", Category = "Mystery", Price = 499.0, stock = 9, Desc = "A wife disappears and the marriage unravels in public.", Year = 2012 },
                new Book { title = "The Girl with the Dragon Tattoo", Author = "Stieg Larsson", ISBN = "9780002346238", Category = "Mystery", Price = 525.0, stock = 0, Desc = "A journalist and a hacker investigate a decades-old disappearance.", Year = 2005 },
                new Book { title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780002425421", Category = "Technology", Price = 899.0, stock = 7, Desc = "Principles and practices for writing readable, maintainable code.", Year = 2008 },
                new Book { title = "The Pragmatic Programmer", Author = "Andrew Hunt and David Thomas", ISBN = "9780002504614", Category = "Technology", Price = 949.0, stock = 6, Desc = "Pragmatic advice for software craftspeople.", Year = 1999 },
                new Book { title = "Designing Data-Intensive Applications", Author = "Martin Kleppmann", ISBN = "9780002583800", Category = "Technology", Price = 1299.0, stock = 5, Desc = "The big ideas behind reliable, scalable, maintainable systems.", Year = 2017 },
                new Book { title = "Domain-Driven Design", Author = "Eric Evans", ISBN = "9780002662994", Category = "Technology", Price = 1450.0, stock = 3, Desc = "Tackling complexity in the heart of software.", Year = 2003 },
                new Book { title = "Refactoring", Author = "Martin Fowler", ISBN = "9780002742184", Category = "Technology", Price = 1099.0, stock = 4, Desc = "Improving the design of existing code, one small step at a time.", Year = 1999 },
                new Book { title = "The Lean Startup", Author = "Eric Ries", ISBN = "9780002821377", Category = "Business", Price = 499.0, stock = 12, Desc = "Build, measure, learn: a method for building startups.", Year = 2011 },
                new Book { title = "Good to Great", Author = "Jim Collins", ISBN = "9780002900560", Category = "Business", Price = 599.0, stock = 8, Desc = "Why some companies make the leap and others don't.", Year = 2001 },
                new Book { title = "Thinking, Fast and Slow", Author = "Daniel Kahneman", ISBN = "9780002979757", Category = "Business", Price = 650.0, stock = 10, Desc = "The two systems that drive the way we think.", Year = 2011 },
                new Book { title = "Sapiens", Author = "Yuval Noah Harari", ISBN = "9780003058949", Category = "History", Price = 599.0, stock = 14, Desc = "A brief history of humankind.", Year = 2011 },
                new Book { title = "Guns, Germs, and Steel", Author = "Jared Diamond", ISBN = "9780003138139", Category = "History", Price = 625.0, stock = 6, Desc = "The fates of human societies.", Year = 1997 },
                new Book { title = "The Discovery of India", Author = "Jawaharlal Nehru", ISBN = "9780003217322", Category = "History", Price = 399.0, stock = 9, Desc = "A sweeping account of Indian history and culture.", Year = 1946 },
                new Book { title = "Atomic Habits", Author = "James Clear", ISBN = "9780003296518", Category = "Self-Help", Price = 549.0, stock = 22, Desc = "Tiny changes, remarkable results.", Year = 2018 },
            });
            db.SaveChanges();
        }
    }
}
