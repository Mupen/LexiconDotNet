namespace ReactNews.Application.Contracts.Sources;

/// <summary>
/// What: Raw source-list filter request from the API layer.
/// How: Carries nullable category, language, and country values exactly as query strings may provide them.
/// Why: The use case owns validation/normalization, keeping controllers thin and predictable.
/// </summary>
public sealed record SourceListRequest(
    string? Category,
    string? Language,
    string? Country);
