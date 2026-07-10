namespace ReactNews.Application.Contracts.ReaderPreferences;

/// <summary>
/// What: Request contract for replacing reader preferences.
/// How: Allows nullable values so the application layer can apply defaults and validation in one place.
/// Why: HTTP input is untrusted; controllers should pass it through instead of owning preference rules.
/// </summary>
public sealed record UpdateReaderPreferencesRequest(
    string? Theme,
    decimal? FontScale,
    bool? CompactCards,
    IReadOnlyList<string>? PreferredCategories);
