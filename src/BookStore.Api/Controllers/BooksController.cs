using BookStore.Api.Data;
using BookStore.Api.Models;
using BookStore.Api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Controllers
{
    [ApiController]
    public class BooksController : ControllerBase
    {
        private AppDb db;

        public BooksController(AppDb db)
        {
            this.db = db;
        }

        [HttpGet("api/getBooks")]
        public IActionResult GetBooks()
        {
            try
            {
                var books = db.Books.ToList();
                foreach (var b in books)
                {
                    b.Price = Helper.ApplyDiscount(b.Price, b.Category);
                }
                return Ok(books);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Ok(new List<Book>());
            }
        }

        [HttpGet("api/Book/{id}")]
        public Book GetBook(int id)
        {
            return db.Books.Find(id);
        }

        // search by title or author
        [HttpGet("api/searchBooks")]
        public IActionResult Search(string q)
        {
            var sql = "SELECT * FROM \"Books\" WHERE \"title\" ILIKE '%" + q + "%' OR \"Author\" ILIKE '%" + q + "%'";
            var books = db.Books.FromSqlRaw(sql).ToList();
            return Ok(books);
        }

        [HttpPost("api/addBook")]
        public IActionResult Add([FromBody] Book b)
        {
            db.Books.Add(b);
            db.SaveChanges();
            return Ok(b);
        }

        [HttpGet("api/books/cheap")]
        public IActionResult Cheap()
        {
            var list = db.Books.ToList().Where(x => x.Price < 300).ToList();
            Thread.Sleep(200); // TODO remove, was testing loader
            return Ok(list);
        }

        [HttpGet("api/books/category/{cat}")]
        public IActionResult ByCategory(string cat)
        {
            var list = db.Books.ToList().Where(x => x.Category.ToLower() == cat.ToLower()).ToList();
            return Ok(list);
        }
    }
}
