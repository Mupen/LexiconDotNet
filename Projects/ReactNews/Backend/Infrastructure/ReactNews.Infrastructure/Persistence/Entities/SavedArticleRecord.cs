namespace ReactNews.Infrastructure.Persistence.Entities;

/// <summary>
/// What: Database row for one article saved by one reader.
/// How: Stores the article display fields plus SavedAtUnixTimeMilliseconds for ordering.
/// Why: Saved articles must survive app restarts and should not depend on temporary article snapshot expiry.
/// </summary>
public sealed class SavedArticleRecord
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string ArticleId { get; set; } = string.Empty;

    public string? SourceName { get; set; }

    public string? Author { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string? Content { get; set; }

    public long SavedAtUnixTimeMilliseconds { get; set; }
}
