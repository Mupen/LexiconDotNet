using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Domain.Entities.EditorialArticles;

/// <summary>
/// What: Represents a ReactNews-owned article created by an admin/editor.
/// How: Stores editorial text, metadata, lifecycle status, and audit timestamps.
/// Why: First-party editorial content is different from external NewsAPI content and needs its own domain model.
/// </summary>
public sealed record EditorialArticle(
    string Id,
    string Title,
    string Slug,
    string Summary,
    string Body,
    string Author,
    string Category,
    string? ImageUrl,
    EditorialArticleStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? PublishedAtUtc);
