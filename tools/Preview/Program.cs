using CodingWiki_Model.Models;
using Microsoft.AspNetCore.Mvc;

var root = PreviewSetup.Root("CodingWiki_Web"); var web = typeof(CodingWiki_Web.Controllers.HomeController).Assembly;
var builder = PreviewSetup.Builder(root, 8841, Path.Combine(root, "CodingWiki_Web", "wwwroot"), web.GetName().Name);
builder.Services.AddControllersWithViews().AddApplicationPart(web).ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new PreviewControllers()));
var app = builder.Build(); PreviewSetup.Guard(app, true); app.UseStaticFiles(); app.MapControllerRoute("preview", "{controller=Preview}/{action=Index}"); app.MapGet("/Home", () => Results.Redirect("/")); app.MapGet("/Book", () => Results.Redirect("/")); app.MapGet("/Author", () => Results.Redirect("/Preview/Authors")); app.Run();

public sealed class PreviewController : Controller
{
    [HttpGet] public IActionResult Index() => View("~/Views/Book/Index.cshtml", books());
    [HttpGet]
    public IActionResult Details(int id)
    { var book = books().SingleOrDefault(b => b.BookId == id); return book is null ? NotFound() : View("~/Views/Book/Details.cshtml", new BookDetail { Book = book, Book_Id = id, BookDetail_Id = id, NumberOfChapters = 12, NumberOfPages = 320, Weight = "Sample 450 g" }); }
    [HttpGet] public IActionResult Authors() => View("~/Views/Author/Index.cshtml", authors());
    private static Author[] authors() => new[] { new Author { Author_Id = 1, FirstName = "Sample", LastName = "Writer", Location = "Fixture city", BirthDate = new DateTime(1990, 1, 1) }, new Author { Author_Id = 2, FirstName = "Second", LastName = "Writer", Location = "Fixture city", BirthDate = new DateTime(1992, 1, 1) } };
    private static Book[] books() => Enumerable.Range(1, 3).Select(i => new Book { BookId = i, Title = $"EF relationship sample {i}", ISBN = $"FIXTURE-{i}", Price = 20 + i * 5, Publisher_Id = 1, Publisher = new Publisher { Publisher_Id = 1, Name = "Fixture publisher" }, BookAuthorMap = authors().Select(a => new BookAuthorMap { Book_Id = i, Author_Id = a.Author_Id, Author = a }).ToList() }).ToArray();
}
