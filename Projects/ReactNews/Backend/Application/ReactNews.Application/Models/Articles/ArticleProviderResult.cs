using ReactNews.Domain.Entities.Articles;

namespace ReactNews.Application.Models.Articles;

/// <summary>
/// What: Result returned by infrastructure before the application formats the API response.
/// How: Wraps domain articles together with paging total, fetch time, cache expiry, and cache-origin flag.
/// Why: The application layer should not know whether articles came from HTTP, memory cache, or another store, but it still needs metadata for responses.
/// </summary>
public sealed record ArticleProviderResult(
    IReadOnlyList<Article> Articles,
    int TotalResults,
    DateTimeOffset FetchedAtUtc,
    DateTimeOffset CachedUntilUtc,
    bool FromCache);
