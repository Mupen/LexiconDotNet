namespace ReactNews.Application.Contracts.Articles;

/// <summary>
/// What: Response contract for the normalized /api/articles endpoint.
/// How: Contains the article DTO list plus paging, mode, cache, and timestamp metadata.
/// Why: The frontend needs more than articles; it needs enough metadata to render pagination and explain whether data came from cache.
/// </summary>
public sealed record ArticleListResponse(
    IReadOnlyList<NewsArticleDto> Items,
    int TotalResults,
    int TotalPages,
    int Page,
    int PageSize,
    string Mode,
    bool FromCache,
    DateTimeOffset FetchedAtUtc,
    DateTimeOffset CachedUntilUtc);
