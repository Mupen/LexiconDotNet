namespace ReactNews.Infrastructure.Persistence.Entities;

/// <summary>
/// Database record for one article snapshot returned from NewsAPI.
/// </summary>
/// <remarks>
/// What: stores the article fields ReactNews needs for detail pages.
/// Why: NewsAPI does not provide a stable "get by id" endpoint for this app, so
/// ReactNews must remember article data it has already shown to the user.
/// How: the generated article id is the primary key. Newer fetches overwrite the
/// same id and extend the expiry time.
/// </remarks>
public sealed class ArticleSnapshotRecord
{
    public string Id { get; set; } = string.Empty;

    public string? SourceName { get; set; }

    public string? Author { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string? Content { get; set; }

    public long ExpiresAtUnixTimeMilliseconds { get; set; }

    public long StoredAtUnixTimeMilliseconds { get; set; }
}
