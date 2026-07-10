using ReactNews.Domain.Entities.Articles;

namespace ReactNews.Domain.Entities.SavedArticles;

/// <summary>
/// What: Represents an article intentionally saved by a reader.
/// How: Wraps the normalized Article entity together with the UTC time when it was saved.
/// Why: Saved articles are not just cache snapshots; they are user intent and should be modeled as their own domain concept.
/// </summary>
public sealed record SavedArticle(
    Article Article,
    DateTimeOffset SavedAtUtc);
