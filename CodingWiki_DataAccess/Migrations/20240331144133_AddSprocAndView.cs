using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodingWiki_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSprocAndView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR ALTER VIEW dbo.GetMainBookDetails
                AS
                SELECT  m.ISBN,m.Title,m.Price FROM dbo.Books m
            ");

            migrationBuilder.Sql(@"CREATE OR ALTER VIEW dbo.getAllBookDetails
                AS
                SELECT * FROM dbo.Books m
            ");

            migrationBuilder.Sql(@"CREATE PROCEDURE dbo.getBookDetailById
                    @bookId int
                AS   
                    SET NOCOUNT ON;  
                    SELECT  *  FROM dbo.Books b
	                WHERE b.BookId=@bookId
                GO  
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.GetMainBookDetails");
            migrationBuilder.Sql("DROP VIEW dbo.getAllBookDetails");
            migrationBuilder.Sql("DROP PROCEDURE dbo.getBookDetailById");
        }
    }
}
