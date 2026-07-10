using ReactNews.Application.Contracts.Common;

using ReactNews.Application.Interfaces;

namespace ReactNews.Application.Queries.SavedArticles;

/// <summary>
/// What: Removes one article from the reader's saved list.
/// How: Delegates deletion to ISavedArticleStore and returns a Result that describes success or not-found.
/// Why: Remove is user intent and should be handled as an application use case instead of direct controller/database code.
/// </summary>
public sealed class RemoveSavedArticle
{
    private readonly ISavedArticleStore _savedArticleStore;

    public RemoveSavedArticle(ISavedArticleStore savedArticleStore)
    {
        _savedArticleStore = savedArticleStore;
    }

    /// <summary>
    /// What: Executes removal for one article id.
    /// How: Calls the store and returns true when a saved row was removed, or not_found when nothing matched.
    /// Why: The API can map this to HTTP 200/404 while keeping database details outside controllers.
    /// </summary>
    public Result<bool> Execute(string userId, string articleId)
    {
        return _savedArticleStore.Remove(userId, articleId)
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(Error.NotFound("Saved article was not found."));
    }
}
