using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Gets one public editorial article by id.
/// How: Looks up the article and returns a DTO only when its status is Published.
/// Why: A known id should not let readers open drafts or archived articles.
/// </summary>
public sealed class GetPublishedEditorialArticleById
{
    private readonly IEditorialArticleStore _store;

    public GetPublishedEditorialArticleById(IEditorialArticleStore store)
    {
        _store = store;
    }

    /// <summary>
    /// What: Returns the article for public detail pages or null when unavailable.
    /// How: Treats missing, Draft, and Archived articles the same from the
    /// public API's point of view.
    /// Why: Public routes should not reveal private workflow state by saying
    /// "this exists but is draft"; a normal not-found response is cleaner.
    /// </summary>
    public EditorialArticleDto? Execute(string id)
    {
        var article = _store.Find(id);

        return article?.Status == EditorialArticleStatus.Published
            ? article.ToDto()
            : null;
    }
}
