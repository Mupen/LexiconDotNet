using Microsoft.Extensions.Caching.Memory;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.Sources;
using ReactNews.Infrastructure.Caching.NewsProviders;
using ReactNews.Infrastructure.Providers;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests the caching wrapper around the real news-feed client.
/// How: Uses an in-memory MemoryCache, a fake provider source, and a fake snapshot store to observe calls and cached results.
/// Why: Caching is infrastructure behavior that can easily hide bugs, so these tests prove when the inner client is called and when it is skipped.
/// </summary>
public sealed class CachedNewsFeedClientTests
{
    /// <summary>
    /// What: Checks that the first article request is fetched from the inner provider.
    /// How: Runs one request through the cached client and asserts the fake provider was called once.
    /// Why: The cache cannot invent data; the first request for a key must load from the real provider source.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_CallsInnerProvider_OnFirstRequest()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var innerProvider = new FakeNewsProviderSource();
        var snapshotStore = new FakeArticleSnapshotStore();
        var decorator = new CachedNewsFeedClient(innerProvider, cache, snapshotStore);

        var result = await decorator.GetArticlesAsync(CreateQuery(page: 1), CancellationToken.None);

        Assert.False(result.FromCache);
        Assert.Equal(1, innerProvider.ArticleCallCount);
        Assert.Single(result.Articles);
    }

    /// <summary>
    /// What: Checks that repeated article requests use the cached value.
    /// How: Runs the same query twice and verifies the second result is marked FromCache with no second provider call.
    /// Why: This protects the NewsAPI quota and improves response time for repeated frontend filters.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_ReturnsCachedResult_OnSecondSameRequest()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var innerProvider = new FakeNewsProviderSource();
        var snapshotStore = new FakeArticleSnapshotStore();
        var decorator = new CachedNewsFeedClient(innerProvider, cache, snapshotStore);
        var query = CreateQuery(page: 1);

        var firstResult = await decorator.GetArticlesAsync(query, CancellationToken.None);
        var secondResult = await decorator.GetArticlesAsync(query, CancellationToken.None);

        Assert.False(firstResult.FromCache);
        Assert.True(secondResult.FromCache);
        Assert.Equal(1, innerProvider.ArticleCallCount);
    }

    /// <summary>
    /// What: Checks that returned articles are also remembered as snapshots.
    /// How: Executes an article request and verifies the fake snapshot store received the article and expiration time.
    /// Why: The detail page needs a way to load an article by id after list results have been shown.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_RemembersSnapshots_WhenProviderReturnsArticles()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var innerProvider = new FakeNewsProviderSource();
        var snapshotStore = new FakeArticleSnapshotStore();
        var decorator = new CachedNewsFeedClient(innerProvider, cache, snapshotStore);

        await decorator.GetArticlesAsync(CreateQuery(page: 1), CancellationToken.None);

        Assert.Single(snapshotStore.Articles);
        Assert.Equal("article-1", snapshotStore.Articles[0].Id);
        Assert.True(snapshotStore.LastExpiresAtUtc > DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// What: Checks that different article queries do not share the same cache entry.
    /// How: Requests page 1 and page 2 and verifies the inner provider was called twice.
    /// Why: Cache keys must include the query values so users do not see page 1 data when they asked for page 2.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_UsesDifferentCacheKeys_ForDifferentQueries()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var innerProvider = new FakeNewsProviderSource();
        var snapshotStore = new FakeArticleSnapshotStore();
        var decorator = new CachedNewsFeedClient(innerProvider, cache, snapshotStore);

        await decorator.GetArticlesAsync(CreateQuery(page: 1), CancellationToken.None);
        await decorator.GetArticlesAsync(CreateQuery(page: 2), CancellationToken.None);

        Assert.Equal(2, innerProvider.ArticleCallCount);
    }

    /// <summary>
    /// What: Checks that repeated source requests are cached.
    /// How: Calls GetSourcesAsync twice with the same filters and verifies only one provider call happened.
    /// Why: Source lists are stable enough to cache, which reduces repeated provider calls for the same filter UI.
    /// </summary>
    [Fact]
    public async Task GetSourcesAsync_ReturnsCachedResult_OnSecondSameRequest()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var innerProvider = new FakeNewsProviderSource();
        var snapshotStore = new FakeArticleSnapshotStore();
        var decorator = new CachedNewsFeedClient(innerProvider, cache, snapshotStore);

        var firstResult = await decorator.GetSourcesAsync("technology", "en", "us", CancellationToken.None);
        var secondResult = await decorator.GetSourcesAsync("technology", "en", "us", CancellationToken.None);

        Assert.False(firstResult.FromCache);
        Assert.True(secondResult.FromCache);
        Assert.Equal(1, innerProvider.SourceCallCount);
    }

    /// <summary>
    /// What: Creates a reusable article query for cache-key tests.
    /// How: Builds the same headline query every time while allowing the page value to change.
    /// Why: Varying only one field makes it clear which cache-key difference the test is proving.
    /// </summary>
    private static ArticleQuery CreateQuery(int page)
    {
        return new ArticleQuery(
            Mode: "headlines",
            Query: null,
            Country: "us",
            Category: "technology",
            Source: null,
            Language: "en",
            SortBy: "publishedAt",
            Page: page,
            PageSize: 20);
    }

    /// <summary>
    /// What: Provides a fake lower-level provider source for cache tests.
    /// How: Counts method calls and returns deterministic article/source result objects.
    /// Why: The cached client should be tested against a predictable dependency so the call-count assertions are meaningful.
    /// </summary>
    private sealed class FakeNewsProviderSource : INewsProviderSource
    {
        public int ArticleCallCount { get; private set; }

        public int SourceCallCount { get; private set; }

        public Task<ArticleProviderResult> GetArticlesAsync(
            ArticleQuery query,
            CancellationToken cancellationToken)
        {
            // What: Simulates loading articles from the provider source.
            // How: Increments a call counter and creates one article whose id depends on the requested page.
            // Why: The page-specific id makes accidental cache-key sharing visible in test failures.
            ArticleCallCount++;

            var article = new Article(
                Id: $"article-{query.Page}",
                SourceName: "Test Source",
                Author: "Author",
                Title: $"Article {query.Page}",
                Description: "Description",
                Url: $"https://example.com/articles/{query.Page}",
                ImageUrl: null,
                PublishedAt: DateTimeOffset.UtcNow,
                Content: "Content");

            return Task.FromResult(new ArticleProviderResult(
                new[] { article },
                TotalResults: 2,
                FetchedAtUtc: DateTimeOffset.UtcNow,
                CachedUntilUtc: DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }

        /// <summary>
        /// What: Simulates loading source data from the underlying provider source.
        /// How: increments a call counter and echoes the requested filters into the
        /// returned source result.
        /// Why: cache tests need to prove that repeated requests reuse cached source
        /// data while different filter values produce different cache entries.
        /// </summary>
        public Task<SourceProviderResult> GetSourcesAsync(
            string? category,
            string? language,
            string? country,
            CancellationToken cancellationToken)
        {
            // What: Simulates loading source data from the provider source.
            // How: Increments a call counter and echoes the requested filters into the returned source.
            // Why: Echoing filters makes it easier to see whether the cached client passed the expected values through.
            SourceCallCount++;

            var source = new Source(
                Id: "test-source",
                Name: "Test Source",
                Description: "Description",
                Url: "https://example.com",
                Category: category,
                Language: language,
                Country: country);

            return Task.FromResult(new SourceProviderResult(
                new[] { source },
                FetchedAtUtc: DateTimeOffset.UtcNow,
                CachedUntilUtc: DateTimeOffset.UtcNow.AddMinutes(15),
                FromCache: false));
        }
    }

    /// <summary>
    /// What: Captures article snapshots written by the cached client.
    /// How: Appends remembered articles to a list and records the last expiration time.
    /// Why: The cache test needs to verify snapshot side effects without using Entity Framework or SQLite.
    /// </summary>
    private sealed class FakeArticleSnapshotStore : IArticleSnapshotStore
    {
        public List<Article> Articles { get; } = new();

        public DateTimeOffset LastExpiresAtUtc { get; private set; }

        public void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc)
        {
            // What: Records snapshots passed by the cached client.
            // How: Adds all articles to an in-memory list and saves the expiration timestamp.
            // Why: This lets tests prove the cached client supports detail-page lookup after list fetches.
            Articles.AddRange(articles);
            LastExpiresAtUtc = expiresAtUtc;
        }

        public Article? Find(string id)
        {
            // What: Looks up a remembered article from the fake store.
            // How: Searches the in-memory list by id.
            // Why: This completes the interface even though these cache tests mainly assert writes.
            return Articles.FirstOrDefault(article => article.Id == id);
        }
    }
}
