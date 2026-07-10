namespace ReactNews.Infrastructure.Contracts.NewsApi;

/// <summary>
/// What: Provider-specific source item returned by the NewsAPI sources endpoint.
/// How: Mirrors the source JSON fields used by NewsAPI.
/// Why: Infrastructure maps this external shape into the stable domain Source entity before Application sees it.
/// </summary>
internal sealed record NewsApiSourceItem(
    string? Id,
    string? Name,
    string? Description,
    string? Url,
    string? Category,
    string? Language,
    string? Country);
