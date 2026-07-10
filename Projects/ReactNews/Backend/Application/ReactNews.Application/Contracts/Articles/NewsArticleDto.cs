namespace ReactNews.Application.Contracts.Articles;

/// <summary>
/// Article contract returned to the React frontend.
/// </summary>
/// <remarks>
/// What: this DTO is intentionally shaped for the UI, not for NewsAPI.
/// Why: frontend code should not be coupled to external provider property names
/// or to fields the backend does not want to expose.
/// How: application use cases map domain Article entities into this response
/// model before returning data to the API layer.
/// </remarks>
public sealed record NewsArticleDto(
    string Id,
    string? SourceName,
    string? Author,
    string Title,
    string? Description,
    string Url,
    string? ImageUrl,
    DateTimeOffset? PublishedAt,
    string? Content);
