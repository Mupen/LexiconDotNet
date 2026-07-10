using Microsoft.AspNetCore.Mvc;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Contracts.Articles;
using ReactNews.Application.Exceptions;
using ReactNews.Application.Queries.Articles;

namespace ReactNews.Api.Controllers.Articles;

/// <summary>
/// HTTP endpoints for article browsing, searching, and detail snapshots.
/// </summary>
/// <remarks>
/// What: this controller translates HTTP requests into application use case calls.
/// Why: controllers are the delivery layer in clean architecture. They should
/// know about HTTP, status codes, route names, and query-string binding, but not
/// about HttpClient, NewsAPI URLs, cache keys, or storage dictionaries.
/// How: each action builds a request object or forwards route values, then calls
/// a use case. Known exceptions are mapped to stable HTTP responses for React.
/// </remarks>
[ApiController]
public sealed class ArticlesController : ControllerBase
{
    /// <summary>
    /// Gets the normalized article list used by the current React UI.
    /// </summary>
    /// <remarks>
    /// What: accepts headline/search filters from the query string and returns an
    /// application-shaped article list response.
    /// How: ASP.NET binds query values, the controller wraps them in
    /// ArticleListRequest, GetArticles validates/executes the use case, and
    /// ApiResultMapping converts success or validation failure to HTTP.
    /// Why: this keeps HTTP binding in the API layer and keeps NewsAPI/cache
    /// details outside the controller.
    /// </remarks>
    [HttpGet("/api/articles")]
    public async Task<IResult> GetArticles(
        [FromQuery] string? mode,
        [FromQuery(Name = "q")] string? query,
        [FromQuery] string? country,
        [FromQuery] string? category,
        [FromQuery] string? source,
        [FromQuery] string? language,
        [FromQuery] string? sortBy,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] GetArticles useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await useCase.ExecuteAsync(
                new ArticleListRequest(mode, query, country, category, source, language, sortBy, page, pageSize),
                cancellationToken);

            return ApiResultMapping.ToHttpResult(result);
        }
        catch (Exception ex) when (ex is NewsConfigurationException or NewsProviderException)
        {
            return ApiExceptionMapping.ToProblemResult(ex);
        }
    }

    /// <summary>
    /// Gets one article snapshot by generated article id.
    /// </summary>
    /// <remarks>
    /// What: returns persisted article metadata for the detail page.
    /// How: the route value is passed to GetArticleById, which reads the snapshot
    /// store through the application interface.
    /// Why: NewsAPI has no stable get-by-id endpoint for this project, so the
    /// backend owns snapshot lookup.
    /// </remarks>
    [HttpGet("/api/articles/{articleId}")]
    public IResult GetArticleById(
        string articleId,
        [FromServices] GetArticleById useCase)
    {
        var article = useCase.Execute(articleId);

        return article is null
            ? Results.NotFound(new { error = "Article snapshot was not found. Load headlines or run a search first, then open an article from those results." })
            : Results.Ok(article);
    }
}
