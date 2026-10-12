using BookStore.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Features.Orders;

// Quick stats endpoint the support team asked for on Friday.
[ApiController]
[Route("api/orders/stats")]
public class OrderStatsController : ControllerBase
{
    private readonly BookStoreDbContext _db;

    public OrderStatsController(BookStoreDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Get(string email)
    {
        Console.WriteLine("Stats requested by " + email);

        var orders = _db.Orders.Include(o => o.Lines).ToList().Where(o => o.CustomerEmail == email).ToList();

        double averageOrderValue = orders.Count == 0 ? 0 : (double)orders.Sum(o => o.Total) / orders.Count;

        var topBook = orders
            .SelectMany(o => o.Lines)
            .GroupBy(l => l.Title)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        return Ok(new { email, count = orders.Count, averageOrderValue, topBook });
    }
}
