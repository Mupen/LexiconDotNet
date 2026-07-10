namespace ReactNews.Domain.Entities.Sources;

/// <summary>
/// News source entity used by the backend after provider data has been normalized.
/// </summary>
/// <remarks>
/// What: represents one publisher/source that can be used for filtering articles.
/// Why: source data belongs in the domain because it is part of the problem
/// ReactNews works with, while provider response objects belong in Infrastructure.
/// How: infrastructure maps NewsAPI source responses into this entity before
/// application use cases shape the data for the API response.
/// </remarks>
public sealed record Source(
    string Id,
    string Name,
    string? Description,
    string? Url,
    string? Category,
    string? Language,
    string? Country);
