using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.EditorialArticles;

namespace ReactNews.Application.Queries.EditorialArticles;

/// <summary>
/// What: Creates a new admin-authored editorial article.
/// How: Validates the request, creates a domain article, saves it, and maps the saved result.
/// Why: Article creation is a business use case and should not be direct controller-to-database code.
/// </summary>
public sealed class CreateEditorialArticle
{
    private readonly IEditorialArticleStore _store;

    public CreateEditorialArticle(IEditorialArticleStore store)
    {
        _store = store;
    }

    public Result<EditorialArticleDto> Execute(EditorialArticleRequest request)
    {
        try
        {
            var article = EditorialArticleFactory.Create(request, DateTimeOffset.UtcNow);
            return Result<EditorialArticleDto>.Success(_store.Save(article).ToDto());
        }
        catch (ArgumentException ex)
        {
            return Result<EditorialArticleDto>.Failure(Error.Validation(ex.Message));
        }
    }
}
