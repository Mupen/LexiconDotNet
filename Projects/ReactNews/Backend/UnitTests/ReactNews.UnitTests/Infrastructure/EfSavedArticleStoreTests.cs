using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Storage.SavedArticles;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests the EF Core saved-article store against SQLite.
/// How: Uses one open in-memory SQLite connection shared across DbContext instances.
/// Why: Saved articles are persistent reader data, so tests should prove real relational storage behavior.
/// </summary>
public sealed class EfSavedArticleStoreTests
{
    /// <summary>
    /// What: Checks that saving persists an article and listing can read it from another context.
    /// How: Saves through one context, lists through a second context, and verifies the article id.
    /// Why: This proves data is stored in SQLite, not only tracked by one DbContext.
    /// </summary>
    [Fact]
    public void Save_PersistsSavedArticle()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfSavedArticleStore(context);

        store.Save("user-1", CreateArticle("article-1", "Original title"), DateTimeOffset.UtcNow);

        using var secondContext = database.CreateContext();
        var secondStore = new EfSavedArticleStore(secondContext);
        var savedArticles = secondStore.List("user-1");

        Assert.Single(savedArticles);
        Assert.Equal("article-1", savedArticles[0].Article.Id);
    }

    /// <summary>
    /// What: Checks that saving the same article updates instead of duplicating.
    /// How: Saves the same id twice with different titles and verifies one row remains.
    /// Why: Clicking save twice should be idempotent and keep the newest article metadata.
    /// </summary>
    [Fact]
    public void Save_UpdatesExistingSavedArticle()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfSavedArticleStore(context);

        store.Save("user-1", CreateArticle("article-1", "Original title"), DateTimeOffset.UtcNow);
        store.Save("user-1", CreateArticle("article-1", "Updated title"), DateTimeOffset.UtcNow.AddMinutes(1));

        var savedArticle = Assert.Single(store.List("user-1"));

        Assert.Equal("Updated title", savedArticle.Article.Title);
        Assert.Equal(1, context.SavedArticles.Count());
    }

    /// <summary>
    /// What: Checks that removing a saved article deletes the row.
    /// How: Saves one article, removes it, and verifies the list is empty.
    /// Why: The frontend remove action must permanently update the saved list.
    /// </summary>
    [Fact]
    public void Remove_DeletesSavedArticle()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfSavedArticleStore(context);

        store.Save("user-1", CreateArticle("article-1", "Title"), DateTimeOffset.UtcNow);
        var removed = store.Remove("user-1", "article-1");

        Assert.True(removed);
        Assert.Empty(store.List("user-1"));
    }

    [Fact]
    public void List_ReturnsOnlyArticlesSavedByRequestedUser()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfSavedArticleStore(context);

        store.Save("user-1", CreateArticle("article-1", "User 1 title"), DateTimeOffset.UtcNow);
        store.Save("user-2", CreateArticle("article-1", "User 2 title"), DateTimeOffset.UtcNow);

        var userOneArticles = store.List("user-1");
        var userTwoArticles = store.List("user-2");

        Assert.Single(userOneArticles);
        Assert.Single(userTwoArticles);
        Assert.Equal("User 1 title", userOneArticles[0].Article.Title);
        Assert.Equal("User 2 title", userTwoArticles[0].Article.Title);
        Assert.Equal(2, context.SavedArticles.Count());
    }

    /// <summary>
    /// What: Creates a valid article domain object for saved-article store tests.
    /// How: fills the required article fields with deterministic sample values.
    /// Why: these tests focus on saved-article persistence, not external provider
    /// mapping or article validation.
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

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        /// <summary>
        /// What: Holds the open SQLite connection backing one in-memory database.
        /// How: stores the connection supplied by Create until disposal.
        /// Why: SQLite in-memory databases only live while their connection remains open.
        /// </summary>
        private TestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// What: Creates a fresh relational test database in memory.
        /// How: opens a SQLite connection and creates the EF Core schema.
        /// Why: store tests need relational database behavior without creating files.
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
        /// What: Creates a DbContext over the shared in-memory SQLite database.
        /// How: configures ReactNewsDbContext with SQLite options using the open
        /// connection.
        /// Why: separate contexts verify that saved articles are actually persisted.
        /// </summary>
        public ReactNewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReactNewsDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new ReactNewsDbContext(options);
        }

        /// <summary>
        /// What: Releases the temporary SQLite database.
        /// How: disposes the open connection.
        /// Why: disposing the connection removes the in-memory database after the test.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
