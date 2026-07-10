using Microsoft.AspNetCore.Mvc;
using ReactNews.Application.Queries.EditorialArticles;

namespace ReactNews.Api.Controllers.PublicEditorialArticles;

/// <summary>
/// What: Public HTTP endpoints for published ReactNews-owned editorial articles.
/// How: Delegates feed and detail requests to public-only Application use cases.
/// Why: Admin editorial management and public article reading have different
/// security rules, so they should not share the same controller route.
/// </summary>
[ApiController]
public sealed class PublicEditorialArticlesController : ControllerBase
{
    /// <summary>
    /// What: Returns the public editorial feed.
    /// How: The use case filters out drafts and archived articles before this
    /// controller serializes the response.
    /// Why: Anonymous readers should be able to browse first-party published
    /// articles without needing an Admin account.
    /// </summary>
    [HttpGet("/api/public/editorial/articles")]
    public IResult GetArticles([FromServices] ListPublishedEditorialArticles useCase)
    {
        return Results.Ok(useCase.Execute());
    }

    /// <summary>
    /// What: Returns one published editorial article by id.
    /// How: The use case returns null for missing or unpublished articles, and
    /// this controller maps null to HTTP 404.
    /// Why: Public detail pages need a stable URL while private draft state must
    /// stay hidden from readers.
    /// </summary>
    [HttpGet("/api/public/editorial/articles/{id}")]
    public IResult GetArticle(
        string id,
        [FromServices] GetPublishedEditorialArticleById useCase)
    {
        var article = useCase.Execute(id);

        return article is null
            ? Results.NotFound(new { error = "Published editorial article was not found." })
            : Results.Ok(article);
    }
}
