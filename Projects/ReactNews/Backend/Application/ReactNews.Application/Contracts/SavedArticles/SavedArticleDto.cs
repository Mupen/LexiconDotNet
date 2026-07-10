namespace ReactNews.Application.Contracts.SavedArticles;

/// <summary>
/// What: Article contract returned from saved-article endpoints.
/// How: Combines the normal article display fields with SavedAtUtc metadata.
/// Why: The frontend needs the article data for rendering and the saved timestamp for reader-facing saved-list behavior.
/// </summary>
public sealed record SavedArticleDto(
    string Id,
    string? SourceName,
    string? Author,
    string Title,
    string? Description,
    string Url,
    string? ImageUrl,
    DateTimeOffset? PublishedAt,
    string? Content,
    DateTimeOffset SavedAtUtc);
