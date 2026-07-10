using ReactNews.Application.Contracts.SavedArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.SavedArticles;

/// <summary>
/// What: Lists the articles saved by one authenticated reader.
/// How: Reads saved article domain objects from ISavedArticleStore and maps them to DTOs.
/// Why: Listing saved articles is application behavior; controllers should only expose it over HTTP.
/// </summary>
public sealed class ListSavedArticles
{
    private readonly ISavedArticleStore _savedArticleStore;

    public ListSavedArticles(ISavedArticleStore savedArticleStore)
    {
        _savedArticleStore = savedArticleStore;
    }

    /// <summary>
    /// What: Executes the saved-article list use case.
    /// How: Gets all saved articles for the supplied user id, maps each one to SavedArticleDto, and wraps the result in SavedArticleListResponse.
    /// Why: A response wrapper keeps the API shape consistent and leaves space for paging/user metadata later.
    /// </summary>
    public SavedArticleListResponse Execute(string userId)
    {
        var items = _savedArticleStore
            .List(userId)
            .Select(savedArticle => savedArticle.ToDto())
            .ToList();

        return new SavedArticleListResponse(items);
    }
}
