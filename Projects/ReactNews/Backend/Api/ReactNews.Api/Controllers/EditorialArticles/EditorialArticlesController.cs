using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Queries.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Api.Controllers.EditorialArticles;

/// <summary>
/// What: HTTP endpoints for admin-created editorial articles.
/// How: Delegates list/get/create/update/publish/archive commands to Application use cases.
/// Why: Controllers should expose HTTP routes while editorial workflow and persistence stay in lower layers.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
public sealed class EditorialArticlesController : ControllerBase
{
    /// <summary>
    /// What: Returns all editorial articles for the admin workspace.
    /// How: ListEditorialArticles reads the editorial repository and returns drafts,
    /// published articles, and archived articles in one response.
    /// Why: admins need a management view that includes private article states that
    /// are intentionally hidden from public readers.
    /// </summary>
    [HttpGet("/api/editorial/articles")]
    public IResult GetArticles([FromServices] ListEditorialArticles useCase)
    {
        return Results.Ok(useCase.Execute());
    }

    /// <summary>
    /// What: Returns one editorial article for editing or review.
    /// How: the route id is passed to GetEditorialArticleById, and a missing
    /// article is converted to a 404 response.
    /// Why: edit screens need the complete private article record, but the API
    /// should still give a clear response when an id no longer exists.
    /// </summary>
    [HttpGet("/api/editorial/articles/{id}")]
    public IResult GetArticle(
        string id,
        [FromServices] GetEditorialArticleById useCase)
    {
        var article = useCase.Execute(id);

        return article is null
            ? Results.NotFound(new { error = "Editorial article was not found." })
            : Results.Ok(article);
    }

    /// <summary>
    /// What: Creates a new editorial article draft.
    /// How: ASP.NET binds the request body, CreateEditorialArticle validates it,
    /// and ApiResultMapping converts the application result into HTTP.
    /// Why: new admin-written content should enter the system through the same
    /// validation path that later updates use.
    /// </summary>
    [HttpPost("/api/editorial/articles")]
    public IResult CreateArticle(
        [FromBody] EditorialArticleRequest request,
        [FromServices] CreateEditorialArticle useCase)
    {
        var result = useCase.Execute(request);
        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Updates the content fields for an existing editorial article.
    /// How: the route id selects the stored article and the request body supplies
    /// the replacement title, summary, body, category, image, and source fields.
    /// Why: content edits should be explicit and validated in Application before
    /// Infrastructure persists the updated entity.
    /// </summary>
    [HttpPut("/api/editorial/articles/{id}")]
    public IResult UpdateArticle(
        string id,
        [FromBody] EditorialArticleRequest request,
        [FromServices] UpdateEditorialArticle useCase)
    {
        var result = useCase.Execute(id, request);
        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Changes an editorial article into the published state.
    /// How: the shared status-change use case receives the route id and the
    /// Published enum value.
    /// Why: publishing is a workflow action, so it is represented as a command
    /// endpoint instead of asking the frontend to mutate status fields directly.
    /// </summary>
    [HttpPost("/api/editorial/articles/{id}/publish")]
    public IResult PublishArticle(
        string id,
        [FromServices] ChangeEditorialArticleStatus useCase)
    {
        var result = useCase.Execute(id, EditorialArticleStatus.Published);
        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Changes an editorial article into the archived state.
    /// How: the shared status-change use case receives the route id and the
    /// Archived enum value.
    /// Why: archiving keeps old content available for admin history while removing
    /// it from public editorial lists.
    /// </summary>
    [HttpPost("/api/editorial/articles/{id}/archive")]
    public IResult ArchiveArticle(
        string id,
        [FromServices] ChangeEditorialArticleStatus useCase)
    {
        var result = useCase.Execute(id, EditorialArticleStatus.Archived);
        return ApiResultMapping.ToHttpResult(result);
    }
}
