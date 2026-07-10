using ReactNews.Domain.Entities.Sources;

namespace ReactNews.Application.Models.Sources;

/// <summary>
/// What: Result returned by infrastructure for source-list requests.
/// How: Wraps normalized source entities together with fetch/cache metadata.
/// Why: Source loading has the same cache behavior as article loading, so the application receives a consistent provider result shape.
/// </summary>
public sealed record SourceProviderResult(
    IReadOnlyList<Source> Sources,
    DateTimeOffset FetchedAtUtc,
    DateTimeOffset CachedUntilUtc,
    bool FromCache);
