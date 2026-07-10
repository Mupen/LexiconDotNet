using ReactNews.Application.Interfaces;
using ReactNews.Application.Queries.SavedArticles;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.SavedArticles;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests saved-article application use cases without using a database.
/// How: Uses fake snapshot and saved-article stores to control article availability.
/// Why: Use-case tests should prove business flow before infrastructure or HTTP behavior is involved.
/// </summary>
public sealed class SavedArticleUseCaseTests
{
    /// <summary>
    /// What: Checks that saving an existing snapshot returns a saved article DTO.
    /// How: Seeds the fake snapshot store, executes SaveArticleForLater, and verifies success output.
    /// Why: Saving depends on a remembered article snapshot, so the use case must connect both stores correctly.
    /// </summary>
    [Fact]
    public void SaveArticleForLater_ReturnsSavedArticle_WhenSnapshotExists()
    {
        var snapshotStore = new FakeArticleSnapshotStore();
        snapshotStore.Remember(new[] { CreateArticle("article-1") }, DateTimeOffset.UtcNow.AddMinutes(5));
        var savedStore = new FakeSavedArticleStore();
        var useCase = new SaveArticleForLater(snapshotStore, savedStore);

        var result = useCase.Execute("user-1", "article-1");

        Assert.True(result.IsSuccess);
        Assert.Equal("article-1", result.Value?.Id);
        Assert.Single(savedStore.List("user-1"));
    }

    /// <summary>
    /// What: Checks that saving a missing snapshot returns not_found.
    /// How: Executes SaveArticleForLater with an empty snapshot store.
    /// Why: The API should tell the user to load/search articles first instead of creating empty saved records.
    /// </summary>
    [Fact]
    public void SaveArticleForLater_ReturnsNotFound_WhenSnapshotIsMissing()
    {
        var useCase = new SaveArticleForLater(new FakeArticleSnapshotStore(), new FakeSavedArticleStore());

        var result = useCase.Execute("user-1", "missing");

        Assert.True(result.IsFailure);
        Assert.Equal("not_found", result.Error?.Code);
    }

    /// <summary>
    /// What: Checks that listing saved articles returns DTOs in store order.
    /// How: Saves one article in the fake saved store and executes ListSavedArticles.
    /// Why: The frontend list page depends on this response shape.
    /// </summary>
    [Fact]
    public void ListSavedArticles_ReturnsSavedItems()
    {
        var savedStore = new FakeSavedArticleStore();
        savedStore.Save("user-1", CreateArticle("article-1"), DateTimeOffset.UtcNow);
        var useCase = new ListSavedArticles(savedStore);

        var response = useCase.Execute("user-1");

        Assert.Single(response.Items);
        Assert.Equal("article-1", response.Items[0].Id);
    }

    /// <summary>
    /// What: Checks that removing a saved article reports success.
    /// How: Saves an article in the fake store, removes it through the use case, and verifies the store is empty.
    /// Why: Remove behavior should be explicit and testable before HTTP DELETE maps it to a response.
    /// </summary>
    [Fact]
    public void RemoveSavedArticle_ReturnsSuccess_WhenArticleWasSaved()
    {
        var savedStore = new FakeSavedArticleStore();
        savedStore.Save("user-1", CreateArticle("article-1"), DateTimeOffset.UtcNow);
        var useCase = new RemoveSavedArticle(savedStore);

        var result = useCase.Execute("user-1", "article-1");

        Assert.True(result.IsSuccess);
        Assert.Empty(savedStore.List("user-1"));
    }

    /// <summary>
    /// What: Checks that removing a missing saved article returns not_found.
    /// How: Executes RemoveSavedArticle against an empty fake saved store.
    /// Why: DELETE should not pretend a missing saved article existed.
    /// </summary>
    [Fact]
    public void RemoveSavedArticle_ReturnsNotFound_WhenArticleWasNotSaved()
    {
        var useCase = new RemoveSavedArticle(new FakeSavedArticleStore());

        var result = useCase.Execute("user-1", "missing");

        Assert.True(result.IsFailure);
        Assert.Equal("not_found", result.Error?.Code);
    }

    /// <summary>
    /// What: Builds a complete domain article for saved-article tests.
    /// How: Uses stable values and lets the id vary.
    /// Why: Saved-article use cases need real article data but not repeated setup noise.
    /// </summary>
    private static Article CreateArticle(string id)
    {
        return new Article(
            Id: id,
            SourceName: "Source",
            Author: "Author",
            Title: "Title",
            Description: "Description",
            Url: $"https://example.com/{id}",
            ImageUrl: null,
            PublishedAt: DateTimeOffset.UtcNow,
            Content: "Content");
    }

    /// <summary>
    /// What: In-memory implementation of IArticleSnapshotStore for use-case tests.
    /// How: Stores articles in a dictionary by id.
    /// Why: SaveArticleForLater needs snapshot lookup without using EF Core.
    /// </summary>
    private sealed class FakeArticleSnapshotStore : IArticleSnapshotStore
    {
        private readonly Dictionary<string, Article> _articles = new();

        public void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc)
        {
            foreach (var article in articles)
            {
                _articles[article.Id] = article;
            }
        }

        public Article? Find(string id)
        {
            return _articles.GetValueOrDefault(id);
        }
    }

    /// <summary>
    /// What: In-memory implementation of ISavedArticleStore for use-case tests.
    /// How: Stores saved articles in a dictionary and returns newest first.
    /// Why: Application tests need deterministic persistence behavior without a database.
    /// </summary>
    private sealed class FakeSavedArticleStore : ISavedArticleStore
    {
        private readonly Dictionary<string, SavedArticle> _articles = new();

        public SavedArticle Save(string userId, Article article, DateTimeOffset savedAtUtc)
        {
            var savedArticle = new SavedArticle(article, savedAtUtc);
            _articles[$"{userId}:{article.Id}"] = savedArticle;
            return savedArticle;
        }

        public IReadOnlyList<SavedArticle> List(string userId)
        {
            return _articles
                .Where(article => article.Key.StartsWith($"{userId}:", StringComparison.Ordinal))
                .Select(article => article.Value)
                .OrderByDescending(article => article.SavedAtUtc)
                .ToList();
        }

        /// <summary>
        /// What: Removes one fake saved article for one user.
        /// How: deletes the composite user/article dictionary key.
        /// Why: remove-use-case tests need a state change that mirrors the real
        /// saved-article store contract.
        /// </summary>
        public bool Remove(string userId, string articleId)
        {
            return _articles.Remove($"{userId}:{articleId}");
        }
    }
}
