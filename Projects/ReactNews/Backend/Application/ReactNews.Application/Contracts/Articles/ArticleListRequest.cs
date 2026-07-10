namespace ReactNews.Application.Contracts.Articles;

/// <summary>
/// Raw article list request from the API layer.
/// </summary>
/// <remarks>
/// What: this record accepts nullable/untrusted values exactly like an HTTP
/// query string can provide them.
/// Why: controllers should stay thin and avoid business defaults such as
/// "headlines is the default mode" or "page size must be clamped".
/// How: the GetArticles use case sends this request to ArticleQueryFactory,
/// which validates and converts it into an application ArticleQuery model.
/// </remarks>
public sealed record ArticleListRequest(
    string? Mode,
    string? Query,
    string? Country,
    string? Category,
    string? Source,
    string? Language,
    string? SortBy,
    int? Page,
    int? PageSize);
