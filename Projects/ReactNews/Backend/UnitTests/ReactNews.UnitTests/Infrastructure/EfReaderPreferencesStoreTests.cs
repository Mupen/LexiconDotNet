using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReactNews.Domain.Entities.ReaderPreferences;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Storage.ReaderPreferences;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests EF Core persistence for reader preferences.
/// How: Uses in-memory SQLite with the real ReactNewsDbContext model.
/// Why: Reader preferences should survive DbContext boundaries like real persisted settings.
/// </summary>
public sealed class EfReaderPreferencesStoreTests
{
    [Fact]
    public void Get_CreatesDefaultPreferences_WhenMissing()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfReaderPreferencesStore(context);

        var preferences = store.Get("user-1");

        Assert.Equal("light", preferences.Theme);
        Assert.Single(context.ReaderPreferences);
    }

    /// <summary>
    /// What: Verifies that reader preferences are persisted across DbContext instances.
    /// How: saves preferences in one context, opens another context, and reads the
    /// same user preferences back.
    /// Why: store behavior must prove real database persistence, not only EF change
    /// tracker state.
    /// </summary>
    [Fact]
    public void Save_PersistsPreferences()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfReaderPreferencesStore(context);

        store.Save("user-1", new ReaderPreferences("dark", 1.15m, true, new[] { "sports", "science" }));

        using var secondContext = database.CreateContext();
        var secondStore = new EfReaderPreferencesStore(secondContext);
        var saved = secondStore.Get("user-1");

        Assert.Equal("dark", saved.Theme);
        Assert.Equal(1.15m, saved.FontScale);
        Assert.True(saved.CompactCards);
        Assert.Contains("sports", saved.PreferredCategories);
    }

    /// <summary>
    /// What: Verifies that preference rows are isolated per user id.
    /// How: saves different preferences for two users and reads both back.
    /// Why: one reader's theme/feed settings must not overwrite another reader's
    /// settings.
    /// </summary>
    [Fact]
    public void Save_IsolatesPreferencesByUser()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();
        var store = new EfReaderPreferencesStore(context);

        store.Save("user-1", new ReaderPreferences("dark", 1.15m, true, new[] { "sports" }));
        store.Save("user-2", new ReaderPreferences("light", 0.95m, false, new[] { "technology" }));

        var userOnePreferences = store.Get("user-1");
        var userTwoPreferences = store.Get("user-2");

        Assert.Equal("dark", userOnePreferences.Theme);
        Assert.Equal("light", userTwoPreferences.Theme);
        Assert.Equal(2, context.ReaderPreferences.Count());
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        /// <summary>
        /// What: Holds the open SQLite connection for one test database.
        /// How: stores the connection until Dispose is called.
        /// Why: in-memory SQLite data disappears when its connection is closed.
        /// </summary>
        private TestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// What: Creates a fresh in-memory SQLite database with schema.
        /// How: opens the connection and calls EnsureCreated through a context.
        /// Why: EF store tests need relational behavior without file-based test data.
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
        /// What: Creates a DbContext connected to the shared test database.
        /// How: configures EF Core SQLite options with the open connection.
        /// Why: tests can create multiple contexts over the same temporary schema.
        /// </summary>
        public ReactNewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReactNewsDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new ReactNewsDbContext(options);
        }

        /// <summary>
        /// What: Cleans up the temporary database connection.
        /// How: disposes the SQLite connection.
        /// Why: the test database should be released as soon as the test completes.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
