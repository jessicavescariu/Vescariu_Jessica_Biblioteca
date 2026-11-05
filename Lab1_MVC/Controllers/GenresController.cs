
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_MVC.Data;
using Lab1_MVC.Models;

namespace Lab1_MVC.Controllers
{
    public class GenresController : Controller
    {
        private readonly LibraryContext _context;

        public GenresController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Genre.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var genre = await _context.Genre
                .FirstOrDefaultAsync(g => g.ID == id);

            if (genre == null)
                return NotFound();

            return View(genre);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(genre);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var genre = await _context.Genre.FindAsync(id);

            if (genre == null)
                return NotFound();

            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            if (id != genre.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(genre);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var genre = await _context.Genre
                .FirstOrDefaultAsync(g => g.ID == id);

            if (genre == null)
                return NotFound();

            return View(genre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genre.FindAsync(id);

            if (genre != null)
                _context.Genre.Remove(genre);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
