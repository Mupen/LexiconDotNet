using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.EditorialArticles;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Updates an existing editorial article.
/// How: Finds the current article, validates replacement values, saves the updated article, and returns a DTO.
/// Why: Updates should preserve identity/timestamps while replacing editable editorial fields.
/// </summary>
public sealed class UpdateEditorialArticle
{
    private readonly IEditorialArticleStore _store;

    public UpdateEditorialArticle(IEditorialArticleStore store)
    {
        _store = store;
    }

    public Result<EditorialArticleDto> Execute(string id, EditorialArticleRequest request)
    {
        var existing = _store.Find(id);

        if (existing is null)
        {
            return Result<EditorialArticleDto>.Failure(Error.NotFound("Editorial article was not found."));
        }

        try
        {
            var article = EditorialArticleFactory.Update(existing, request, DateTimeOffset.UtcNow);
            return Result<EditorialArticleDto>.Success(_store.Save(article).ToDto());
        }
        catch (ArgumentException ex)
        {
            return Result<EditorialArticleDto>.Failure(Error.Validation(ex.Message));
        }
    }
}
