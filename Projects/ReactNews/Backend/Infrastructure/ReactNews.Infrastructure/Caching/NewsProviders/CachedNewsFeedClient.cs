using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Infrastructure.Providers;

namespace ReactNews.Infrastructure.Caching.NewsProviders;

/// <summary>
/// Adds caching and article snapshot behavior around an INewsProvider.
/// </summary>
/// <remarks>
/// What: this decorator checks the memory cache before calling the inner news
/// provider, stores successful provider results, and remembers article snapshots
/// for detail pages.
/// Why: cache policy is a separate infrastructure concern from NewsAPI HTTP
/// integration. Keeping it here makes the provider easier to understand and
/// gives us a natural place to replace IMemoryCache with HybridCache, Redis, or
/// another distributed cache later.
/// How: dependency injection creates the raw NewsApiClient, then wraps it
/// with this decorator as the application's INewsProvider implementation.
/// </remarks>
public sealed class CachedNewsFeedClient : INewsProvider
{
    private readonly INewsProviderSource _innerProvider;
    private readonly IMemoryCache _cache;
    private readonly IArticleSnapshotStore _articleSnapshotStore;

    public CachedNewsFeedClient(
        INewsProviderSource innerProvider,
        IMemoryCache cache,
        IArticleSnapshotStore articleSnapshotStore)
    {
        _innerProvider = innerProvider;
        _cache = cache;
        _articleSnapshotStore = articleSnapshotStore;
    }

    /// <summary>
    /// Gets articles with memory caching and snapshot persistence.
    /// </summary>
    /// <remarks>
    /// What: returns cached article results when possible, otherwise calls the raw
    /// news feed client and stores the successful result.
    /// How: a deterministic cache key is built from the full ArticleQuery. Cache
    /// hits return a copy with FromCache set to true. Cache misses call the inner
    /// client, cache the result until the provider expiry, and remember article
    /// snapshots for detail pages.
    /// Why: article-list requests are common and NewsAPI quota is limited. This
    /// keeps repeat requests fast while still separating cache policy from HTTP
    /// provider code.
    /// </remarks>
    public async Task<ArticleProviderResult> GetArticlesAsync(
        ArticleQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildArticleCacheKey(query);

        if (_cache.TryGetValue<ArticleProviderResult>(cacheKey, out var cachedResult) && cachedResult is not null)
        {
            return cachedResult with { FromCache = true };
        }

        var result = await _innerProvider.GetArticlesAsync(query, cancellationToken);

        _cache.Set(
            cacheKey,
            result,
            new MemoryCacheEntryOptions
            {
                SlidingExpiration = GetSlidingExpiration(query.Mode),
                AbsoluteExpiration = result.CachedUntilUtc
            });

        _articleSnapshotStore.Remember(result.Articles, result.CachedUntilUtc);

        return result;
    }

    /// <summary>
    /// Gets source metadata with memory caching.
    /// </summary>
    /// <remarks>
    /// What: returns cached source results when possible, otherwise calls the raw
    /// news feed client.
    /// How: category/language/country values form a readable source cache key.
    /// Cache hits return FromCache true. Cache misses are stored for the provider
    /// expiry with a short sliding expiration.
    /// Why: source metadata changes less frequently than headlines, so caching it
    /// avoids unnecessary external requests.
    /// </remarks>
    public async Task<SourceProviderResult> GetSourcesAsync(
        string? category,
        string? language,
        string? country,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"sources:{category ?? "all"}:{language ?? "all"}:{country ?? "all"}";

        if (_cache.TryGetValue<SourceProviderResult>(cacheKey, out var cachedResult) && cachedResult is not null)
        {
            return cachedResult with { FromCache = true };
        }

        var result = await _innerProvider.GetSourcesAsync(category, language, country, cancellationToken);

        _cache.Set(
            cacheKey,
            result,
            new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpiration = result.CachedUntilUtc
            });

        return result;
    }

    /// <summary>
    /// Builds a stable article cache key for one request.
    /// </summary>
    /// <remarks>
    /// What: includes every field that can affect the article response.
    /// How: values are joined into a raw string and hashed with SHA-256.
    /// Why: hashing keeps cache keys compact and avoids special-character issues
    /// while still ensuring different filters/pages do not share data.
    /// </remarks>
    private static string BuildArticleCacheKey(ArticleQuery query)
    {
        var rawKey = string.Join('|', new[]
        {
            query.Mode,
            query.Query ?? string.Empty,
            query.Country,
            query.Category,
            query.Source ?? string.Empty,
            query.Language,
            query.SortBy,
            query.Page.ToString(CultureInfo.InvariantCulture),
            query.PageSize.ToString(CultureInfo.InvariantCulture)
        });

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return $"articles:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    /// <summary>
    /// Chooses the sliding cache duration for article results.
    /// </summary>
    /// <remarks>
    /// What: returns a short duration based on query mode.
    /// How: search results get a slightly longer sliding window than headlines.
    /// Why: headlines are expected to move quickly, while searches are often
    /// repeated while users refine or revisit the same query.
    /// </remarks>
    private static TimeSpan GetSlidingExpiration(string mode)
    {
        return mode == "search" ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(45);
    }
}
