namespace ReactNews.Infrastructure.Contracts.NewsApi;

/// <summary>
/// What: Provider-specific article item returned by NewsAPI.
/// How: Mirrors the JSON field names used by NewsAPI, including nullable fields for incomplete provider data.
/// Why: Keeping this contract inside Infrastructure prevents NewsAPI's response shape from leaking into Domain or Application.
/// </summary>
internal sealed record NewsApiArticle(
    NewsApiSource? Source,
    string? Author,
    string? Title,
    string? Description,
    string? Url,
    string? UrlToImage,
    DateTimeOffset? PublishedAt,
    string? Content);
