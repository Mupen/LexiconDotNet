using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Changes the workflow status of an editorial article.
/// How: Finds the article, applies the requested status with timestamp rules, saves it, and returns a DTO.
/// Why: Publish/archive commands should be explicit use cases rather than generic field updates.
/// </summary>
public sealed class ChangeEditorialArticleStatus
{
    private readonly IEditorialArticleStore _store;

    public ChangeEditorialArticleStatus(IEditorialArticleStore store)
    {
        _store = store;
    }

    public Result<EditorialArticleDto> Execute(string id, EditorialArticleStatus status)
    {
        var existing = _store.Find(id);

        if (existing is null)
        {
            return Result<EditorialArticleDto>.Failure(Error.NotFound("Editorial article was not found."));
        }

        var article = EditorialArticleFactory.ChangeStatus(existing, status, DateTimeOffset.UtcNow);
        return Result<EditorialArticleDto>.Success(_store.Save(article).ToDto());
    }
}
