using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Lists admin-created editorial articles.
/// How: Reads articles from the editorial store and maps them into DTOs.
/// Why: The admin frontend needs a persisted article table independent of NewsAPI results.
/// </summary>
public sealed class ListEditorialArticles
{
    private readonly IEditorialArticleStore _store;

    public ListEditorialArticles(IEditorialArticleStore store)
    {
        _store = store;
    }

    public EditorialArticleListResponse Execute()
    {
        return new EditorialArticleListResponse(_store.List().Select(article => article.ToDto()).ToList());
    }
}
