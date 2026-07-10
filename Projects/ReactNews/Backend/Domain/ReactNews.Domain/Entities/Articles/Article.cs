namespace ReactNews.Domain.Entities.Articles;

/// <summary>
/// Article entity used by the backend after external provider data has been normalized.
/// </summary>
/// <remarks>
/// What: represents one readable news article inside ReactNews.
/// Why: the backend should not pass raw NewsAPI JSON through the system. External
/// providers can change field names, null behavior, or response quirks, but the
/// domain entity should remain stable for the rest of the application.
/// How: infrastructure maps provider-specific response objects into Article
/// entities before returning them to application use cases.
/// </remarks>
public sealed record Article(
    string Id,
    string? SourceName,
    string? Author,
    string Title,
    string? Description,
    string Url,
    string? ImageUrl,
    DateTimeOffset? PublishedAt,
    string? Content);
