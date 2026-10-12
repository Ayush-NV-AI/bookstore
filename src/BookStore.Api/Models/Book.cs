namespace BookStore.Api.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Category { get; set; }
        public double Price { get; set; }
        public int stock { get; set; }
        public string Desc { get; set; }
        public int Year { get; set; }
    }
}
