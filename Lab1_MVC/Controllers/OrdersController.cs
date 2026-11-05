
using Lab1_MVC.Data;
using Lab1_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lab1_MVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly LibraryContext _context;

        public OrdersController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? customerId)
        {
            var orders = _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .AsQueryable();

            if (customerId.HasValue)
            {
                orders = orders.Where(o => o.CustomerID == customerId.Value);
            }

            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name),
                "ID",
                "Name",
                customerId
            );

            return View(await orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name), "ID", "Name");

            ViewData["BookID"] = new SelectList(
                _context.Book.OrderBy(b => b.Title), "ID", "Title");

            return View(new Order { OrderDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CustomerID,BookID,OrderDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name), "ID", "Name",
                order.CustomerID);

            ViewData["BookID"] = new SelectList(
                _context.Book.OrderBy(b => b.Title), "ID", "Title",
                order.BookID);

            return View(order);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .FirstOrDefaultAsync(o => o.ID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order.FindAsync(id);

            if (order == null)
                return NotFound();

            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name), "ID", "Name",
                order.CustomerID);

            ViewData["BookID"] = new SelectList(
                _context.Book.OrderBy(b => b.Title), "ID", "Title",
                order.BookID);

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ID,CustomerID,BookID,OrderDate")] Order order)
        {
            if (id != order.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name), "ID", "Name",
                order.CustomerID);

            ViewData["BookID"] = new SelectList(
                _context.Book.OrderBy(b => b.Title), "ID", "Title",
                order.BookID);

            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .FirstOrDefaultAsync(o => o.ID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order != null)
                _context.Order.Remove(order);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
