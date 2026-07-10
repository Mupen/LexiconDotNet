using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Contracts.Articles;

namespace ReactNews.Application.Queries.Articles;

/// <summary>
/// Returns a previously loaded article snapshot by its generated id.
/// </summary>
/// <remarks>
/// What: loads one article snapshot and maps it to the detail DTO.
/// How: the application depends on IArticleSnapshotStore, so Infrastructure can
/// decide whether snapshots live in SQLite, memory, or another store.
/// Why: NewsAPI has no get-by-id endpoint for this project, so article detail is
/// a ReactNews-owned snapshot lookup.
/// </remarks>
public sealed class GetArticleById
{
    private readonly IArticleSnapshotStore _articleSnapshotStore;

    public GetArticleById(IArticleSnapshotStore articleSnapshotStore)
    {
        _articleSnapshotStore = articleSnapshotStore;
    }

    /// <summary>
    /// Finds and maps a detail article.
    /// </summary>
    /// <remarks>
    /// What: returns a NewsArticleDto when the snapshot exists, or null when it
    /// does not.
    /// How: the store returns a domain Article and ToDto maps it for API output.
    /// Why: a missing snapshot is an expected case, so null lets the API return
    /// 404 without using exceptions for normal control flow.
    /// </remarks>
    public NewsArticleDto? Execute(string articleId)
    {
        var article = _articleSnapshotStore.Find(articleId);

        return article?.ToDto();
    }
}
