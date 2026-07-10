using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.SavedArticles;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// What: Stores and retrieves articles saved by one authenticated reader.
/// How: Application use cases call this interface while Infrastructure provides the database implementation.
/// Why: Saved-article persistence is a business feature, but EF Core and SQLite details must stay outside Application.
/// </summary>
public interface ISavedArticleStore
{
    /// <summary>
    /// What: Saves an article for later reading by one user.
    /// How: The concrete store inserts or updates the saved article row by user id and article id.
    /// Why: Saving the same article twice should be idempotent per user, while different users need separate saved lists.
    /// </summary>
    SavedArticle Save(string userId, Article article, DateTimeOffset savedAtUtc);

    /// <summary>
    /// What: Lists every article saved by one user.
    /// How: The concrete store returns saved articles ordered by most recently saved first.
    /// Why: A saved-reading list should show the newest saved items first for normal reader workflow.
    /// </summary>
    IReadOnlyList<SavedArticle> List(string userId);

    /// <summary>
    /// What: Removes a saved article by id for one user.
    /// How: The concrete store deletes the matching row and reports whether a row existed.
    /// Why: The API needs to distinguish a successful remove from a request for an article that was not saved.
    /// </summary>
    bool Remove(string userId, string articleId);
}
