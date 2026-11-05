using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab1_MVC.Models;
using Lab1_MVC.Data;
using System.Diagnostics;

namespace Lab1_MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(LibraryContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Books


        public async Task<IActionResult> Index(
        string sortOrder,
        string currentFilter,
        string searchString,
        int? pageNumber,
        int? authorId)
        {
            ViewData["CurrentSort"] = sortOrder;

            ViewData["TitleSortParm"] =
                String.IsNullOrEmpty(sortOrder) ? "title_desc" : "title";

            ViewData["PriceSortParm"] =
                sortOrder == "price" ? "price_desc" : "price";

            ViewData["AuthorSortParm"] =
                sortOrder == "author" ? "author_desc" : "author";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentAuthor"] = authorId;

            ViewData["AuthorID"] = new SelectList(
                _context.Author.OrderBy(a => a.LastName),
                "ID",
                "FullName",
                authorId
            );

            var books = _context.Book
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.Orders)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }

            if (authorId.HasValue)
            {
                books = books.Where(b => b.AuthorID == authorId.Value);
            }

            switch (sortOrder)
            {
                case "title":
                    books = books.OrderBy(b => b.Title);
                    break;

                case "title_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;

                case "price":
                    books = books.OrderBy(b => b.Price);
                    break;

                case "price_desc":
                    books = books.OrderByDescending(b => b.Price);
                    break;

                case "author":
                    books = books
                        .OrderBy(b => b.Author.LastName)
                        .ThenBy(b => b.Author.FirstName);
                    break;

                case "author_desc":
                    books = books
                        .OrderByDescending(b => b.Author.LastName)
                        .ThenByDescending(b => b.Author.FirstName);
                    break;

                default:
                    books = books.OrderByDescending(b => b.Title);
                    break;
            }

            var rows = books.Select(b => new BookListItem(
                b.ID,
                b.Title,
                b.Author != null
                    ? b.Author.FirstName + " " + b.Author.LastName
                    : "",
                b.Genre != null ? b.Genre.Name : "",
                b.Price,
                b.Orders.Count
            ));

            const int pageSize = 9;

            return View(await PaginatedList<BookListItem>.CreateAsync(
                rows,
                pageNumber ?? 1,
                pageSize
            ));
        }


        // GET: Books/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.Orders)
                    .ThenInclude(o => o.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }


        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name");
            ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "FullName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,AuthorID,Price,GenreID")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName");
            ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name");
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName");
            ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name");
            var vm = new BookEditViewModel
            {
                ID = book.ID, Title = book.Title, Price = book.Price,
                AuthorID = book.AuthorID, GenreID = book.GenreID
            };
            return View(vm);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookEditViewModel vm)
        {
            if (id != vm.ID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["AuthorID"] = new SelectList(_context.Author.OrderBy(a => a.LastName), "ID", "FullName", book.AuthorID);
                ViewData["GenreID"] = new SelectList(_context.Genre.OrderBy(g => g.Name), "ID", "Name", book.GenreID);
                return View(vm);
            }
            var book = await _context.Book.FindAsync(id);
            if (book is null) return NotFound();
            book.Title = vm.Title; book.Price = vm.Price;
            book.AuthorID = vm.AuthorID; book.GenreID = vm.GenreID;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.ID == id);
        }
    }
}
