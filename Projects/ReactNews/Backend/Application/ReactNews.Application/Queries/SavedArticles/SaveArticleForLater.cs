using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.SavedArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.SavedArticles;

/// <summary>
/// What: Saves an article snapshot into the reader's saved list.
/// How: Finds the article in the snapshot store, then persists it through ISavedArticleStore.
/// Why: NewsAPI has no direct article-by-id endpoint, so an article must first exist as a ReactNews snapshot before it can be saved.
/// </summary>
public sealed class SaveArticleForLater
{
    private readonly IArticleSnapshotStore _articleSnapshotStore;
    private readonly ISavedArticleStore _savedArticleStore;

    public SaveArticleForLater(
        IArticleSnapshotStore articleSnapshotStore,
        ISavedArticleStore savedArticleStore)
    {
        _articleSnapshotStore = articleSnapshotStore;
        _savedArticleStore = savedArticleStore;
    }

    /// <summary>
    /// What: Executes the save operation for one article id.
    /// How: Looks up the article snapshot, returns not_found if missing, otherwise saves and maps the saved article DTO.
    /// Why: Save requests should be idempotent for existing articles and clear for missing/expired snapshots.
    /// </summary>
    public Result<SavedArticleDto> Execute(string userId, string articleId)
    {
        var article = _articleSnapshotStore.Find(articleId);

        if (article is null)
        {
            return Result<SavedArticleDto>.Failure(Error.NotFound("Article snapshot was not found. Load headlines or run a search first, then save an article from those results."));
        }

        var savedArticle = _savedArticleStore.Save(userId, article, DateTimeOffset.UtcNow);

        return Result<SavedArticleDto>.Success(savedArticle.ToDto());
    }
}
