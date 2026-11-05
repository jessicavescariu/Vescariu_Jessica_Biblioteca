using Lab1_MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1_MVC.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly LibraryContext _context;

        public StatisticsController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await _context.Book.CountAsync();

            ViewBag.TotalAuthors = await _context.Author.CountAsync();

            ViewBag.TotalCustomers = await _context.Customer.CountAsync();

            ViewBag.TotalOrders = await _context.Order.CountAsync();

            ViewBag.AveragePrice = await _context.Book
                .AverageAsync(b => (double?)b.Price) ?? 0;

            ViewBag.MostOrderedBooks = await _context.Book
                .Select(b => new
                {
                    b.Title,
                    OrderCount = b.Orders.Count
                })
                .OrderByDescending(b => b.OrderCount)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}