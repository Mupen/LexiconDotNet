using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Gets one editorial article by id.
/// How: Looks up the article through IEditorialArticleStore and maps it when present.
/// Why: Edit/preview routes need stable article lookup separate from external article snapshots.
/// </summary>
public sealed class GetEditorialArticleById
{
    private readonly IEditorialArticleStore _store;

    public GetEditorialArticleById(IEditorialArticleStore store)
    {
        _store = store;
    }

    public EditorialArticleDto? Execute(string id)
    {
        return _store.Find(id)?.ToDto();
    }
}
