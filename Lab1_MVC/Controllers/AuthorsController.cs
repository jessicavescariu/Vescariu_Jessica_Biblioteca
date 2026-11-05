
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_MVC.Data;
using Lab1_MVC.Models;

namespace Lab1_MVC.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly LibraryContext _context;

        public AuthorsController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Author.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .Include(a => a.Books)
                    .ThenInclude(b => b.Genre)
                .Include(a => a.Books)
                    .ThenInclude(b => b.Orders)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id);
            var distinctGenres = await _context.Book
                .Where(b => b.AuthorID == id && b.Genre != null)
                .Select(b => b.Genre.Name)
                .Distinct()
                .CountAsync();

            ViewBag.DistinctGenres = distinctGenres;
            if (author == null)
                return NotFound();

            return View(author);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author.FindAsync(id);

            if (author == null)
                return NotFound();

            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .FirstOrDefaultAsync(a => a.ID == id);

            if (author == null)
                return NotFound();

            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Author.FindAsync(id);

            if (author != null)
                _context.Author.Remove(author);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
