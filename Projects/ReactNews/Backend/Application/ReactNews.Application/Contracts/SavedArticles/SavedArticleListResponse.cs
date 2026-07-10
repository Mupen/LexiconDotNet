namespace ReactNews.Application.Contracts.SavedArticles;

/// <summary>
/// What: Response contract for listing saved articles.
/// How: Returns saved article DTOs in a single Items collection.
/// Why: Keeping a wrapper response matches the existing article/source list style and leaves room for paging later.
/// </summary>
public sealed record SavedArticleListResponse(IReadOnlyList<SavedArticleDto> Items);
