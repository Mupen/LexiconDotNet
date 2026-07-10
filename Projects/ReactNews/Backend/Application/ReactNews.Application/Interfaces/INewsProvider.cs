using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// Abstraction over the external news system.
/// </summary>
/// <remarks>
/// What: this interface describes the news data the application needs.
/// Why: use cases should not depend on HttpClient, API keys, query-string
/// formatting, caching, or third-party DTOs. Those are infrastructure details.
/// How: the Infrastructure project implements this interface with NewsAPI.org.
/// Unit tests can later provide a fake implementation with deterministic data.
/// </remarks>
public interface INewsProvider
{
    /// <summary>
    /// What: Loads articles that match an already-validated application query.
    /// How: Infrastructure decides whether to call a remote provider, return cache data, or use another backing service.
    /// Why: Use cases should depend on this stable abstraction instead of provider-specific HTTP code.
    /// </summary>
    Task<ArticleProviderResult> GetArticlesAsync(ArticleQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// What: Loads available news sources for optional filters.
    /// How: The concrete provider receives nullable filters and returns normalized source entities with cache metadata.
    /// Why: Source filtering is part of the news browsing feature, but provider-specific endpoints belong outside Application.
    /// </summary>
    Task<SourceProviderResult> GetSourcesAsync(
        string? category,
        string? language,
        string? country,
        CancellationToken cancellationToken);
}
