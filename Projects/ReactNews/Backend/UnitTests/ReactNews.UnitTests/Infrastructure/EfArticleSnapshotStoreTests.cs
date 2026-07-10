using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Storage.ArticleSnapshots;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests the Entity Framework implementation of the article snapshot store.
/// How: Uses an in-memory SQLite connection so EF Core still talks to a real relational provider during the test.
/// Why: Snapshot persistence is important for article detail pages, and SQLite testing is closer to production behavior than a pure fake store.
/// </summary>
public sealed class EfArticleSnapshotStoreTests
{
    /// <summary>
    /// What: Checks that a remembered article can be read back from a new DbContext.
    /// How: Saves an article with one context, creates a second context over the same in-memory connection, and reads by id.
    /// Why: Using a second context proves the data was persisted through EF instead of only being tracked in one context instance.
    /// </summary>
    [Fact]
    public void Remember_PersistsArticleSnapshot()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfArticleSnapshotStore(context);
        var article = CreateArticle("article-1", "Original title");

        store.Remember(new[] { article }, DateTimeOffset.UtcNow.AddMinutes(10));

        using var secondContext = database.CreateContext();
        var secondStore = new EfArticleSnapshotStore(secondContext);
        var savedArticle = secondStore.Find("article-1");

        Assert.NotNull(savedArticle);
        Assert.Equal("Original title", savedArticle.Title);
    }

    /// <summary>
    /// What: Checks that saving the same article id updates the existing snapshot.
    /// How: Saves one title, saves the same id with another title, and verifies one row remains with the new title.
    /// Why: News articles may appear in multiple fetches, so the store should refresh the snapshot instead of duplicating records.
    /// </summary>
    [Fact]
    public void Remember_UpdatesExistingSnapshot()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfArticleSnapshotStore(context);

        store.Remember(new[] { CreateArticle("article-1", "Original title") }, DateTimeOffset.UtcNow.AddMinutes(10));
        store.Remember(new[] { CreateArticle("article-1", "Updated title") }, DateTimeOffset.UtcNow.AddMinutes(10));

        var savedArticle = store.Find("article-1");

        Assert.NotNull(savedArticle);
        Assert.Equal("Updated title", savedArticle.Title);
        Assert.Equal(1, context.ArticleSnapshots.Count());
    }

    /// <summary>
    /// What: Checks that expired snapshots are not returned and are cleaned up.
    /// How: Saves an already-expired article, calls Find, and verifies both a null result and an empty database table.
    /// Why: Detail snapshots are temporary cache data; expired rows should not keep old article details available forever.
    /// </summary>
    [Fact]
    public void Find_ReturnsNullAndDeletesSnapshot_WhenSnapshotExpired()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfArticleSnapshotStore(context);

        store.Remember(new[] { CreateArticle("article-1", "Expired title") }, DateTimeOffset.UtcNow.AddMinutes(-1));

        var savedArticle = store.Find("article-1");

        Assert.Null(savedArticle);
        Assert.Empty(context.ArticleSnapshots);
    }

    /// <summary>
    /// What: Builds a complete article entity for persistence tests.
    /// How: Uses stable sample values for all required fields while allowing id and title to vary.
    /// Why: Persistence tests need realistic complete entities, but repeated construction would distract from the test behavior.
    /// </summary>
    private static Article CreateArticle(string id, string title)
    {
        return new Article(
            Id: id,
            SourceName: "Source",
            Author: "Author",
            Title: title,
            Description: "Description",
            Url: $"https://example.com/{id}",
            ImageUrl: null,
            PublishedAt: DateTimeOffset.UtcNow,
            Content: "Content");
    }

    /// <summary>
    /// What: Owns a temporary SQLite database connection for one test.
    /// How: Keeps a single open in-memory SQLite connection and creates DbContext instances that share it.
    /// Why: SQLite in-memory databases exist only while the connection stays open, so this helper keeps the database alive for the test.
    /// </summary>
    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection)
        {
            // What: Stores the already-open SQLite connection.
            // How: Assigns the constructor argument to a field used by CreateContext and Dispose.
            // Why: The helper must control the connection lifetime so the in-memory database survives across contexts.
            _connection = connection;
        }

        /// <summary>
        /// What: Creates an empty test database with the ReactNews schema applied.
        /// How: Opens an in-memory SQLite connection, builds the helper, creates a DbContext, and calls EnsureCreated.
        /// Why: Each test gets an isolated real relational database without writing files to the project folder.
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
        /// What: Creates a DbContext connected to the test database.
        /// How: Builds DbContextOptions with UseSqlite using the shared open connection.
        /// Why: Multiple contexts are needed to prove persistence across EF tracking boundaries inside the same test.
        /// </summary>
        public ReactNewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReactNewsDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new ReactNewsDbContext(options);
        }

        /// <summary>
        /// What: Releases the SQLite connection after the test.
        /// How: Disposes the connection owned by the helper.
        /// Why: Disposing the connection also removes the in-memory database and prevents resource leaks between tests.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
