
using Lab1_MVC.Data;
using Lab1_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace Lab1_MVC.Services
{
    public class RecommenderTrainer
    {
        private readonly LibraryContext db;
        private readonly IWebHostEnvironment env;

        public RecommenderTrainer(LibraryContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public async Task TrainAsync()
        {
            var ratings = await db.Rating
                .Select(r => new BookRating
                {
                    UserId = r.ReaderId,
                    BookId = r.BookID,
                    Rating = r.Stars
                })
                .ToListAsync();

            if (ratings.Count == 0)
                throw new InvalidOperationException(
                    "Nu există evaluări pentru antrenarea modelului.");

            var ml = new MLContext(seed: 1);

            var data = ml.Data.LoadFromEnumerable(ratings);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(BookRating.UserId),
                MatrixRowIndexColumnName = nameof(BookRating.BookId),
                LabelColumnName = nameof(BookRating.Rating),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var pipeline = ml.Transforms.Conversion
                .MapValueToKey("UserIdEncoded", nameof(BookRating.UserId))
                .Append(ml.Transforms.Conversion
                    .MapValueToKey("BookIdEncoded", nameof(BookRating.BookId)))
                .Append(ml.Recommendation().Trainers.MatrixFactorization(options));

            var model = pipeline.Fit(data);

            var folder = Path.Combine(env.ContentRootPath, "MLModels");

            Directory.CreateDirectory(folder);

            var modelPath = Path.Combine(folder, "BookRecommender.zip");

            ml.Model.Save(model, data.Schema, modelPath);
        }
    }
}
