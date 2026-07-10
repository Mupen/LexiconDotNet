using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Lists editorial articles that are safe for public readers.
/// How: Reads all editorial articles from the store, keeps only Published
/// articles, orders newest published articles first, and maps them to DTOs.
/// Why: Admins need to see drafts and archived articles, but public readers
/// must only receive content that has intentionally been published.
/// </summary>
public sealed class ListPublishedEditorialArticles
{
    private readonly IEditorialArticleStore _store;

    public ListPublishedEditorialArticles(IEditorialArticleStore store)
    {
        _store = store;
    }

    /// <summary>
    /// What: Builds the public editorial feed response.
    /// How: Filters by EditorialArticleStatus.Published before mapping so the
    /// API layer cannot accidentally expose drafts through serialization.
    /// Why: Publication state is application behavior, so it belongs in a use
    /// case instead of being hidden inside the controller.
    /// </summary>
    public EditorialArticleListResponse Execute()
    {
        var articles = _store.List()
            .Where(article => article.Status == EditorialArticleStatus.Published)
            .OrderByDescending(article => article.PublishedAtUtc ?? article.UpdatedAtUtc)
            .Select(article => article.ToDto())
            .ToList();

        return new EditorialArticleListResponse(articles);
    }
}
