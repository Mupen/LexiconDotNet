using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Storage.EditorialArticles;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests EF Core persistence for editorial articles.
/// How: Uses in-memory SQLite with the real ReactNewsDbContext model.
/// Why: Editorial articles are first-party persisted content and must survive DbContext boundaries.
/// </summary>
public sealed class EfEditorialArticleStoreTests
{
    [Fact]
    public void Save_PersistsEditorialArticle()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfEditorialArticleStore(context);

        store.Save(CreateArticle("article-1", "Original title"));

        using var secondContext = database.CreateContext();
        var secondStore = new EfEditorialArticleStore(secondContext);
        var saved = secondStore.Find("article-1");

        Assert.NotNull(saved);
        Assert.Equal("Original title", saved.Title);
    }

    /// <summary>
    /// What: Verifies that saving an existing editorial id updates instead of duplicating.
    /// How: saves two articles with the same id and checks the title plus row count.
    /// Why: the EF store should behave like an upsert for Application save calls.
    /// </summary>
    [Fact]
    public void Save_UpdatesExistingEditorialArticle()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfEditorialArticleStore(context);

        store.Save(CreateArticle("article-1", "Original title"));
        store.Save(CreateArticle("article-1", "Updated title"));

        Assert.Equal("Updated title", store.Find("article-1")?.Title);
        Assert.Equal(1, context.EditorialArticles.Count());
    }

    /// <summary>
    /// What: Creates a valid editorial article domain object for persistence tests.
    /// How: fills required fields and derives a simple slug from the supplied title.
    /// Why: store tests should focus on EF persistence behavior, not article
    /// validation rules handled by the Application factory.
    /// </summary>
    private static EditorialArticle CreateArticle(string id, string title)
    {
        var now = DateTimeOffset.UtcNow;

        return new EditorialArticle(
            Id: id,
            Title: title,
            Slug: title.ToLowerInvariant().Replace(' ', '-'),
            Summary: "Summary",
            Body: "Long editorial body",
            Author: "Admin",
            Category: "technology",
            ImageUrl: null,
            Status: EditorialArticleStatus.Draft,
            CreatedAtUtc: now,
            UpdatedAtUtc: now,
            PublishedAtUtc: null);
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        /// <summary>
        /// What: Keeps the shared in-memory SQLite connection alive for one test.
        /// How: stores the already-open connection until Dispose is called.
        /// Why: SQLite in-memory databases are tied to the connection lifetime.
        /// </summary>
        private TestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// What: Creates an initialized in-memory SQLite database for EF tests.
        /// How: opens a SQLite connection, creates a DbContext, and calls EnsureCreated.
        /// Why: each test gets a real relational database without writing files.
        /// </summary>
        public static TestDatabase Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var database = new TestDatabase(connection);

            using var context = database.CreateContext();
            context.Database.EnsureCreated();

            return database;
        }

        /// <summary>
        /// What: Creates a ReactNewsDbContext connected to the shared test database.
        /// How: builds SQLite options using the open connection.
        /// Why: separate contexts can verify persistence across EF context instances.
        /// </summary>
        public ReactNewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReactNewsDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new ReactNewsDbContext(options);
        }

        /// <summary>
        /// What: Releases the in-memory SQLite database.
        /// How: disposes the open SQLite connection.
        /// Why: disposing the connection cleans up the temporary database after the test.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
