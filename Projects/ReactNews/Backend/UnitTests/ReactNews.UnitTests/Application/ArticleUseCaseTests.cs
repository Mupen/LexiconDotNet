using ReactNews.Application.Contracts.Articles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Application.Queries.Articles;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.Sources;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests the article-related application use cases without calling the real infrastructure layer.
/// How: Uses fake implementations of the news provider and snapshot store to control inputs and observe outputs.
/// Why: Application tests should prove validation, mapping, and orchestration rules while staying independent from HTTP, SQLite, and NewsAPI.
/// </summary>
public sealed class ArticleUseCaseTests
{
    /// <summary>
    /// What: Checks that a search request with a too-short query fails validation.
    /// How: Runs the GetArticles use case with query text shorter than the accepted minimum and asserts a validation error result.
    /// Why: This prevents bad search requests from reaching the external news client, where the error would be slower and harder to explain.
    /// </summary>
    [Fact]
    public async Task GetArticles_ReturnsValidationFailure_WhenSearchQueryIsTooShort()
    {
        var useCase = new GetArticles(new FakeNewsProvider());

        var result = await useCase.ExecuteAsync(
            new ArticleListRequest("search", "x", "us", "technology", null, "en", "publishedAt", 1, 20),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    /// <summary>
    /// What: Checks that article provider entities are mapped into response DTOs.
    /// How: Seeds the fake provider with one article, runs the use case, and verifies both response data and the normalized provider query.
    /// Why: The use case owns the contract between backend business flow and API response shape, so the mapping is worth testing directly.
    /// </summary>
    [Fact]
    public async Task GetArticles_ReturnsMappedArticleResponse()
    {
        var provider = new FakeNewsProvider();
        provider.Articles.Add(CreateArticle("article-1"));
        var useCase = new GetArticles(provider);

        var result = await useCase.ExecuteAsync(
            new ArticleListRequest("headlines", null, "us", "technology", null, "en", "publishedAt", 1, 20),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal("article-1", result.Value.Items[0].Id);
        Assert.Equal("headlines", provider.LastArticleQuery?.Mode);
        Assert.Equal("technology", provider.LastArticleQuery?.Category);
    }

    /// <summary>
    /// What: Checks that the article detail use case can return a previously remembered article snapshot.
    /// How: Stores a fake article in the fake snapshot store, executes the lookup by id, and verifies the returned DTO.
    /// Why: NewsAPI article lists do not give us a separate stable detail endpoint, so the app relies on snapshots for detail pages.
    /// </summary>
    [Fact]
    public void GetArticleById_ReturnsMappedArticle_WhenSnapshotExists()
    {
        var store = new FakeArticleSnapshotStore();
        store.Remember(new[] { CreateArticle("article-1") }, DateTimeOffset.UtcNow.AddMinutes(5));
        var useCase = new GetArticleById(store);

        var article = useCase.Execute("article-1");

        Assert.NotNull(article);
        Assert.Equal("article-1", article.Id);
        Assert.Equal("Test title", article.Title);
    }

    /// <summary>
    /// What: Checks that a missing snapshot returns null instead of throwing.
    /// How: Uses an empty snapshot store and asks for an id that does not exist.
    /// Why: A missing article detail is a normal not-found case, so controllers can map null to HTTP 404 cleanly.
    /// </summary>
    [Fact]
    public void GetArticleById_ReturnsNull_WhenSnapshotIsMissing()
    {
        var useCase = new GetArticleById(new FakeArticleSnapshotStore());

        var article = useCase.Execute("missing");

        Assert.Null(article);
    }

    /// <summary>
    /// What: Builds a complete domain article for tests.
    /// How: Fills every required Article value with predictable sample data while allowing the id to vary.
    /// Why: A shared helper keeps tests focused on behavior instead of repeating long article construction blocks.
    /// </summary>
    private static Article CreateArticle(string id)
    {
        return new Article(
            Id: id,
            SourceName: "Test Source",
            Author: "Test Author",
            Title: "Test title",
            Description: "Description",
            Url: $"https://example.com/{id}",
            ImageUrl: "https://example.com/image.jpg",
            PublishedAt: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Content: "Content");
    }

    /// <summary>
    /// What: Provides a controllable news provider for article use-case tests.
    /// How: Stores seeded articles, records the last query, and returns provider result objects without using HTTP.
    /// Why: The use case should be tested in isolation from the real NewsApiClient and external network behavior.
    /// </summary>
    private sealed class FakeNewsProvider : INewsProvider
    {
        public List<Article> Articles { get; } = new();

        public ArticleQuery? LastArticleQuery { get; private set; }

        public Task<ArticleProviderResult> GetArticlesAsync(
            ArticleQuery query,
            CancellationToken cancellationToken)
        {
            // What: Records the article query and returns the seeded articles.
            // How: Copies the query into LastArticleQuery before wrapping the in-memory list in ArticleProviderResult.
            // Why: Tests need to assert both the mapped response and the normalized request sent to the provider boundary.
            LastArticleQuery = query;

            return Task.FromResult(new ArticleProviderResult(
                Articles,
                Articles.Count,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }

        /// <summary>
        /// What: Provides the source half of INewsProvider for article tests.
        /// How: returns an empty source result because these tests only exercise
        /// article retrieval behavior.
        /// Why: a complete fake provider avoids a mocking framework while keeping
        /// the test focused on article use cases.
        /// </summary>
        public Task<SourceProviderResult> GetSourcesAsync(
            string? category,
            string? language,
            string? country,
            CancellationToken cancellationToken)
        {
            // What: Satisfies the INewsProvider interface for tests that do not need sources.
            // How: Returns an empty source result immediately.
            // Why: Keeping one simple fake avoids adding a mocking framework for this focused use-case test.
            return Task.FromResult(new SourceProviderResult(
                Array.Empty<Source>(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }
    }

    /// <summary>
    /// What: Provides an in-memory snapshot store for article-detail tests.
    /// How: Stores articles in a dictionary by id and returns them through the same interface used by the real store.
    /// Why: Article detail behavior can be tested without creating a real database when persistence behavior is not the focus.
    /// </summary>
    private sealed class FakeArticleSnapshotStore : IArticleSnapshotStore
    {
        private readonly Dictionary<string, Article> _articles = new();

        public void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc)
        {
            // What: Saves article snapshots by id.
            // How: Iterates through the articles and replaces any existing value with the same id.
            // Why: This mirrors the real store's overwrite behavior closely enough for use-case tests.
            foreach (var article in articles)
            {
                _articles[article.Id] = article;
            }
        }

        public Article? Find(string id)
        {
            // What: Looks up a remembered article by id.
            // How: Uses Dictionary.GetValueOrDefault so missing ids become null.
            // Why: Null is the application contract for not-found article details.
            return _articles.GetValueOrDefault(id);
        }
    }
}
