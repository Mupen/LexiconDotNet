using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactNews.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Author = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ExpiresAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    StoredAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorialArticles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreatedAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    PublishedAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReaderPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FontScale = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    CompactCards = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreferredCategories = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReaderPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedArticles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ArticleId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Author = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    SavedAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    CreatedAtUnixTimeMilliseconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleSnapshots_ExpiresAtUnixTimeMilliseconds",
                table: "ArticleSnapshots",
                column: "ExpiresAtUnixTimeMilliseconds");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleSnapshots_Url",
                table: "ArticleSnapshots",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticles_Slug",
                table: "EditorialArticles",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticles_Status",
                table: "EditorialArticles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialArticles_UpdatedAtUnixTimeMilliseconds",
                table: "EditorialArticles",
                column: "UpdatedAtUnixTimeMilliseconds");

            migrationBuilder.CreateIndex(
                name: "IX_ReaderPreferences_UserId",
                table: "ReaderPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedArticles_SavedAtUnixTimeMilliseconds",
                table: "SavedArticles",
                column: "SavedAtUnixTimeMilliseconds");

            migrationBuilder.CreateIndex(
                name: "IX_SavedArticles_Url",
                table: "SavedArticles",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_SavedArticles_UserId_ArticleId",
                table: "SavedArticles",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleSnapshots");

            migrationBuilder.DropTable(
                name: "EditorialArticles");

            migrationBuilder.DropTable(
                name: "ReaderPreferences");

            migrationBuilder.DropTable(
                name: "SavedArticles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
