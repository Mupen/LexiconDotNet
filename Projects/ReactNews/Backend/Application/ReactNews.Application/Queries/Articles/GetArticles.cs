using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.Articles;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Services.Articles;

namespace ReactNews.Application.Queries.Articles;

/// <summary>
/// Main use case for the React article list/table screen.
/// </summary>
/// <remarks>
/// What: this use case gets either top headlines or search results.
/// Why: the React frontend needs one stable backend endpoint even though NewsAPI
/// has separate endpoints for headlines and full-text search.
/// How: raw request values are normalized into ArticleQuery, passed through the
/// INewsProvider abstraction, then mapped into the response shape expected by the
/// frontend.
/// </remarks>
public sealed class GetArticles
{
    private readonly INewsProvider _newsProvider;

    public GetArticles(INewsProvider newsProvider)
    {
        _newsProvider = newsProvider;
    }

    /// <summary>
    /// Executes the normalized article-list use case.
    /// </summary>
    /// <remarks>
    /// What: validates the request, loads article data, maps articles to DTOs,
    /// and returns list metadata for the frontend.
    /// How: ArticleQueryFactory converts raw request values into ArticleQuery;
    /// INewsProvider supplies articles; NewsMapping converts domain articles to
    /// response DTOs.
    /// Why: expected validation failures are returned as Result failures instead
    /// of exceptions, while infrastructure failures still bubble up for API
    /// exception mapping.
    /// </remarks>
    public async Task<Result<ArticleListResponse>> ExecuteAsync(
        ArticleListRequest request,
        CancellationToken cancellationToken)
    {
        ArticleQuery query;

        try
        {
            query = ArticleQueryFactory.Create(
                request.Mode,
                request.Query,
                request.Country,
                request.Category,
                request.Source,
                request.Language,
                request.SortBy,
                request.Page,
                request.PageSize);
        }
        catch (ArgumentException ex)
        {
            return Result<ArticleListResponse>.Failure(Error.Validation(ex.Message));
        }

        var providerResult = await _newsProvider.GetArticlesAsync(query, cancellationToken);
        var articles = providerResult.Articles.Select(article => article.ToDto()).ToList();

        return Result<ArticleListResponse>.Success(new ArticleListResponse(
            Items: articles,
            TotalResults: providerResult.TotalResults,
            TotalPages: (int)Math.Ceiling(providerResult.TotalResults / (double)query.PageSize),
            Page: query.Page,
            PageSize: query.PageSize,
            Mode: query.Mode,
            FromCache: providerResult.FromCache,
            FetchedAtUtc: providerResult.FetchedAtUtc,
            CachedUntilUtc: providerResult.CachedUntilUtc));
    }
}
