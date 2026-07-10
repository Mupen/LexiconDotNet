using ReactNews.Application.Contracts.ReaderPreferences;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Queries.ReaderPreferences;
using ReactNews.Domain.Entities.ReaderPreferences;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests reader-preference use cases without using EF Core.
/// How: Uses an in-memory fake preference store.
/// Why: Preference validation and mapping should be proven independently from database behavior.
/// </summary>
public sealed class ReaderPreferencesUseCaseTests
{
    [Fact]
    public void GetReaderPreferences_ReturnsStoredPreferences()
    {
        var store = new FakeReaderPreferencesStore();
        var useCase = new GetReaderPreferences(store);

        var result = useCase.Execute("user-1");

        Assert.Equal("light", result.Theme);
        Assert.Equal(1.0m, result.FontScale);
        Assert.Contains("technology", result.PreferredCategories);
    }

    /// <summary>
    /// What: Verifies that valid preference changes are saved.
    /// How: executes UpdateReaderPreferences with dark theme, custom font scale,
    /// compact cards, and two categories, then reads the fake store.
    /// Why: the personal feed and display settings depend on persisted preference
    /// updates.
    /// </summary>
    [Fact]
    public void UpdateReaderPreferences_SavesValidPreferences()
    {
        var store = new FakeReaderPreferencesStore();
        var useCase = new UpdateReaderPreferences(store);

        var result = useCase.Execute("user-1", new UpdateReaderPreferencesRequest(
            Theme: "dark",
            FontScale: 1.15m,
            CompactCards: true,
            PreferredCategories: new[] { "sports", "health" }));

        Assert.True(result.IsSuccess);
        Assert.Equal("dark", result.Value?.Theme);
        Assert.True(result.Value?.CompactCards);
        Assert.Equal(2, store.Get("user-1").PreferredCategories.Count);
    }

    /// <summary>
    /// What: Verifies that unsupported theme values fail validation.
    /// How: executes UpdateReaderPreferences with an invalid theme value and
    /// checks the returned validation error.
    /// Why: the frontend can only render supported theme modes, so invalid values
    /// should not be persisted.
    /// </summary>
    [Fact]
    public void UpdateReaderPreferences_ReturnsValidationError_WhenThemeIsInvalid()
    {
        var useCase = new UpdateReaderPreferences(new FakeReaderPreferencesStore());

        var result = useCase.Execute("user-1", new UpdateReaderPreferencesRequest(
            Theme: "blue",
            FontScale: 1.0m,
            CompactCards: false,
            PreferredCategories: new[] { "technology" }));

        Assert.True(result.IsFailure);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    private sealed class FakeReaderPreferencesStore : IReaderPreferencesStore
    {
        private readonly Dictionary<string, ReaderPreferences> _preferencesByUser = new();

        /// <summary>
        /// What: Gets fake preferences for one user.
        /// How: returns stored preferences when present, otherwise returns default
        /// preference values.
        /// Why: tests can verify default behavior without setting up a database row.
        /// </summary>
        public ReaderPreferences Get(string userId)
        {
            if (_preferencesByUser.TryGetValue(userId, out var preferences))
            {
                return preferences;
            }

            return new ReaderPreferences(
                Theme: "light",
                FontScale: 1.0m,
                CompactCards: false,
                PreferredCategories: new[] { "technology", "general" });
        }

        /// <summary>
        /// What: Saves fake preferences for one user.
        /// How: writes the preference object to an in-memory dictionary keyed by user id.
        /// Why: update tests need persisted state that can be inspected after the use
        /// case runs.
        /// </summary>
        public ReaderPreferences Save(string userId, ReaderPreferences preferences)
        {
            _preferencesByUser[userId] = preferences;
            return preferences;
        }
    }
}
