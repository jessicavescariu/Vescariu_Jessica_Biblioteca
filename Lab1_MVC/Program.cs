using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Lab1_MVC.Data;
using Microsoft.Extensions.ML;
using Lab1_MVC.Models;
using Lab1_MVC.Api;
using Scalar.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Lab1_MVC.Services;
using Microsoft.Extensions.ML;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LibraryContext") ?? throw new InvalidOperationException("Connection string 'LibraryContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .AddPredictionEnginePool<BookRating, BookRatingPrediction>()
    .FromFile(modelName: "BookRecommender",
        filePath: "MLModels/BookRecommender.zip",
        watchForChanges: true);
builder.Services
    .AddPredictionEnginePool<BookText, GenrePrediction>()
    .FromFile(
        modelName: "GenreClassifier",
        filePath: "MLModels/GenreClassifier.zip",
        watchForChanges: true);
builder.Services.AddValidation();

builder.Services.AddOpenApi();

var endpoint = new Uri("http://localhost:11434/v1");

IChatClient chat = new OpenAIClient(new ApiKeyCredential("ollama"),
        new OpenAIClientOptions { Endpoint = endpoint })
    .GetChatClient("gemma3:4b")
    .AsIChatClient();

builder.Services.AddChatClient(chat);
builder.Services.AddScoped<RecommenderTrainer>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapLibraryApi();

app.MapPost("/api/v2/books/{id:int}/pitch", async (int id, LibraryContext db, IChatClient ai) =>
{
    var b = await db.Book.Include(x => x.Author).Include(x => x.Genre)
        .FirstOrDefaultAsync(x => x.ID == id);
    if (b is null) return Results.NotFound();

    List<ChatMessage> messages =
    [
        new(ChatRole.System, "Esti bibliotecar. Raspunde in romana, in cel mult 2 propozitii."),
        new(ChatRole.User, $"Recomanda „{b.Title}” de {b.Author?.FullName} " +
                           $"({b.Genre?.Name}). Descriere: {b.Description}")
    ];

    ChatResponse r = await ai.GetResponseAsync(messages);
    return Results.Ok(new { b.Title, Pitch = r.Text });
});

app.MapGet("/api/v2/ask", async (string q, IChatClient ai) =>
    (await ai.GetResponseAsync(q)).Text);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    db.Database.Migrate();
    LibrarySeed.Seed(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "start",
    pattern: "start",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/api/v2/readers/{id:int}/recommendations",
    async (int id, int? top, LibraryContext db,
        PredictionEnginePool<BookRating, BookRatingPrediction> pool) =>
    {
        int take = Math.Clamp(top ?? 5, 1, 20);

        var ratedBookIds = await db.Rating
            .Where(r => r.ReaderId == id)
            .Select(r => r.BookID)
            .ToListAsync();

        var books = await db.Book
            .Where(b => !ratedBookIds.Contains(b.ID))
            .ToListAsync();

        var recommendations = new List<RecommendationDto>();

        try
        {
            foreach (var book in books)
            {
                var prediction = pool.Predict(
                    "BookRecommender",
                    new BookRating
                    {
                        UserId = id,
                        BookId = book.ID
                    });

                recommendations.Add(new RecommendationDto(
                    book.ID,
                    book.Title,
                    prediction.Score,
                    false
                ));
            }
        }
        catch
        {
            recommendations.Clear();
        }

        if (recommendations.Count > 0)
        {
            var result = recommendations
                .OrderByDescending(r => r.Score)
                .Take(take)
                .ToList();

            return Results.Ok(result);
        }

        // Cold Start: minimum 15 evaluări pentru fiecare carte
        var popularBooks = await db.Rating
            .GroupBy(r => r.BookID)
            .Where(g => g.Count() >= 15)
            .Select(g => new
            {
                BookID = g.Key,
                Average = g.Average(r => r.Stars)
            })
            .OrderByDescending(g => g.Average)
            .Take(take)
            .ToListAsync();

        var popularIds = popularBooks.Select(p => p.BookID).ToList();

        var popularTitles = await db.Book
            .Where(b => popularIds.Contains(b.ID))
            .ToDictionaryAsync(b => b.ID, b => b.Title);

        var fallback = popularBooks
            .Where(p => popularTitles.ContainsKey(p.BookID))
            .Select(p => new RecommendationDto(
                p.BookID,
                popularTitles[p.BookID],
                (float)p.Average,
                true
            ))
            .ToList();

        return Results.Ok(fallback);
    });

app.Run();