namespace ReactNews.Application.Contracts.Sources;

/// <summary>
/// What: Response contract for the /api/sources endpoint.
/// How: Contains source DTOs plus cache metadata from the provider result.
/// Why: The frontend can render source options and still understand whether the data was fresh or cached.
/// </summary>
public sealed record SourceListResponse(
    IReadOnlyList<SourceDto> Items,
    bool FromCache,
    DateTimeOffset FetchedAtUtc,
    DateTimeOffset CachedUntilUtc);
