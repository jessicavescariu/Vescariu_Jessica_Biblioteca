
using Lab1_MVC.Data;
using Lab1_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_MVC.Services;

namespace Lab1_MVC.Controllers
{
    //public class AdminController(LibraryContext db) : Controller
    public class AdminController(LibraryContext db, RecommenderTrainer trainer) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.Ratings = await db.Rating.CountAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportRatings(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Msg"] = "Alegeți un fișier CSV.";
                return RedirectToAction(nameof(Index));
            }

            var ratings = new List<Rating>();

            using var reader = new StreamReader(file.OpenReadStream());

            await reader.ReadLineAsync();

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var p = line.Split(',');

                if (p.Length < 3 ||
                    !int.TryParse(p[0], out var u) ||
                    !int.TryParse(p[1], out var b) ||
                    !int.TryParse(p[2], out var s))
                {
                    continue;
                }

                ratings.Add(new Rating
                {
                    ReaderId = u,
                    BookID = b,
                    Stars = s,
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.Rating.AddRange(ratings);
            await db.SaveChangesAsync();

            TempData["Msg"] = $"Au fost importate {ratings.Count} evaluări.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retrain()
        {
            try
            {
                await trainer.TrainAsync();
                TempData["Msg"] = "Modelul de recomandări a fost reantrenat cu succes.";
            }
            catch (Exception ex)
            {
                TempData["Msg"] = "Eroare la antrenare: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Log(
            int? readerId,
            DateTime? from,
            DateTime? to,
            string sortOrder,
            int? pageNumber)
        {
            var logs = db.RecommendationLog.AsQueryable();

            if (readerId.HasValue)
                logs = logs.Where(l => l.ReaderId == readerId.Value);

            if (from.HasValue)
                logs = logs.Where(l => l.CreatedAt >= from.Value);

            if (to.HasValue)
                logs = logs.Where(l => l.CreatedAt < to.Value.AddDays(1));

            logs = sortOrder == "oldest"
                ? logs.OrderBy(l => l.CreatedAt)
                : logs.OrderByDescending(l => l.CreatedAt);

            int pageSize = 10;

            var items = await logs
                .Select(l => new RecommendationLog
                {
                    ID = l.ID,
                    ReaderId = l.ReaderId,
                    CreatedAt = l.CreatedAt,
                    BookIds = l.BookIds
                })
                .ToListAsync();

            var paginated = PaginatedList<RecommendationLog>.Create(
                items, pageNumber ?? 1, pageSize);

            ViewBag.ReaderId = readerId;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.SortOrder = sortOrder;

            return View(paginated);
        }

    }


}
