using System.ComponentModel.DataAnnotations;
using CodingWiki_DataAccess.Data;
using CodingWiki_Model.Models;
using CodingWiki_Model.Models.FluentModels;
using CodingWiki_Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class ModelContractTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("12345678901234567890", true)]
    [InlineData("123456789012345678901", false)]
    public void AnnotatedBookEnforcesIsbnBoundary(string? isbn, bool expected)
    {
        var book = new Book { ISBN = isbn! };
        var context = new ValidationContext(book) { MemberName = nameof(Book.ISBN) };
        Assert.Equal(expected, Validator.TryValidateProperty(isbn, context, new List<ValidationResult>()));
    }

    [Theory]
    [InlineData("", "Writer", false)]
    [InlineData("Sample", "", false)]
    [InlineData("Sample", "Writer", true)]
    public void AuthorRequiresBothNames(string first, string last, bool expected)
    {
        var author = new Author { FirstName = first, LastName = last };
        Assert.Equal(expected, Validator.TryValidateObject(author, new ValidationContext(author), new List<ValidationResult>(), true));
    }

    [Fact]
    public void CompositeMappingKeepsAuthorAndBookIdentity()
    {
        using var context = modelContext();
        foreach (var type in new[] { typeof(BookAuthorMap), typeof(Fluent_BookAuthorMap) })
        {
            var entity = context.Model.FindEntityType(type)!;
            Assert.Equal(new[] { "Author_Id", "Book_Id" }, entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Equal(2, entity.GetForeignKeys().Count());
            Assert.All(entity.GetForeignKeys(), foreignKey => Assert.True(foreignKey.IsRequired));
        }
    }

    [Fact]
    public void AnnotatedAndFluentIsbnContractsRemainDistinct()
    {
        using var context = modelContext();
        var annotated = context.Model.FindEntityType(typeof(Book))!.FindProperty(nameof(Book.ISBN))!;
        var fluent = context.Model.FindEntityType(typeof(Fluent_Book))!.FindProperty(nameof(Fluent_Book.ISBN))!;
        Assert.False(annotated.IsNullable);
        Assert.False(fluent.IsNullable);
        Assert.Equal(20, annotated.GetMaxLength());
        Assert.Equal(50, fluent.GetMaxLength());
    }

    [Fact]
    public void PricePrecisionAndComputedFieldsKeepTheirStorageContracts()
    {
        using var context = modelContext();
        var book = context.Model.FindEntityType(typeof(Book))!;
        Assert.Equal(10, book.FindProperty(nameof(Book.Price))!.GetPrecision());
        Assert.Equal(5, book.FindProperty(nameof(Book.Price))!.GetScale());
        Assert.Null(book.FindProperty(nameof(Book.PriceRange)));
        Assert.Null(context.Model.FindEntityType(typeof(Author))!.FindProperty(nameof(Author.FullName)));
    }

    [Fact]
    public void BookDetailsUseUniqueRequiredBookReference()
    {
        using var context = modelContext();
        var foreignKey = Assert.Single(context.Model.FindEntityType(typeof(BookDetail))!.GetForeignKeys());
        Assert.True(foreignKey.IsUnique);
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(nameof(BookDetail.Book_Id), Assert.Single(foreignKey.Properties).Name);
    }

    [Fact]
    public void MainBookViewRemainsKeylessAndMappedToView()
    {
        using var context = modelContext();
        var entity = context.Model.FindEntityType(typeof(MainBookDetails))!;
        Assert.Null(entity.FindPrimaryKey());
        Assert.Equal("GetMainBookDetails", entity.GetViewName());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void MissingDetailIdentifierReturnsNotFoundWithoutQueryingDatabase(int? id)
    {
        using var context = modelContext();
        Assert.IsType<NotFoundResult>(new BookController(context).Details(id));
    }

    private static ApplicationDbContext modelContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer().Options);
}
