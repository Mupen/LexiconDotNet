namespace ReactNews.Application.Contracts.Sources;

/// <summary>
/// What: Source contract returned to the React frontend.
/// How: Exposes normalized source fields that are useful for filter UI and display.
/// Why: The frontend should depend on the backend's stable source shape, not directly on NewsAPI source JSON.
/// </summary>
public sealed record SourceDto(
    string Id,
    string Name,
    string? Description,
    string? Url,
    string? Category,
    string? Language,
    string? Country);
