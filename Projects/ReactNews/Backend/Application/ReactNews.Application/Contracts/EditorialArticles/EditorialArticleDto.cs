namespace ReactNews.Application.Contracts.EditorialArticles;

/// <summary>
/// What: API contract for one admin-created editorial article.
/// How: Exposes article content, metadata, status, and timestamps as frontend-friendly values.
/// Why: The frontend editorial workspace should depend on a stable API DTO, not EF records or domain internals.
/// </summary>
public sealed record EditorialArticleDto(
    string Id,
    string Title,
    string Slug,
    string Summary,
    string Body,
    string Author,
    string Category,
    string? ImageUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? PublishedAtUtc);
