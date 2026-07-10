namespace ReactNews.Application.Contracts.EditorialArticles;

/// <summary>
/// What: Response contract for listing editorial articles.
/// How: Wraps editorial article DTOs in an Items collection.
/// Why: A wrapper response matches the existing ReactNews list endpoint style and leaves room for paging later.
/// </summary>
public sealed record EditorialArticleListResponse(IReadOnlyList<EditorialArticleDto> Items);
