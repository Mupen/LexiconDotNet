namespace ReactNews.Infrastructure.Persistence.Entities;

/// <summary>
/// What: Database row for one admin-created editorial article.
/// How: Stores content, workflow status, and timestamp fields as SQLite-friendly values.
/// Why: Editorial articles are first-party content and must persist independently from NewsAPI snapshots.
/// </summary>
public sealed class EditorialArticleRecord
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string Status { get; set; } = string.Empty;

    public long CreatedAtUnixTimeMilliseconds { get; set; }

    public long UpdatedAtUnixTimeMilliseconds { get; set; }

    public long? PublishedAtUnixTimeMilliseconds { get; set; }
}
