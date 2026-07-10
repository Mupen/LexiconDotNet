using ReactNews.Domain.Entities.Articles;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// Stores short-lived article snapshots so a user can open a detail page.
/// </summary>
/// <remarks>
/// What: NewsAPI list responses contain enough data for a detail screen, but
/// NewsAPI does not provide a stable "get article by id" endpoint for this app.
/// Why: the frontend needs an article id to route to /articles/{id}; the backend
/// therefore keeps a temporary snapshot of articles that were already returned
/// from a list/search response.
/// How: infrastructure provides the concrete storage implementation. The
/// application depends only on this interface, so the app can use memory,
/// SQLite, or another database without changing controllers or use cases.
/// </remarks>
public interface IArticleSnapshotStore
{
    /// <summary>
    /// What: Stores articles temporarily for later detail-page lookup.
    /// How: The concrete store saves each article by id and associates it with an expiration timestamp.
    /// Why: Article detail routes need stable ids even though NewsAPI does not provide a direct get-by-id endpoint.
    /// </summary>
    void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc);

    /// <summary>
    /// What: Finds a previously remembered article by id.
    /// How: The concrete store returns the article when it exists and is still valid, otherwise null.
    /// Why: Returning null gives the API a clean way to produce a not-found response for expired or unknown article ids.
    /// </summary>
    Article? Find(string id);
}
