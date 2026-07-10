namespace ReactNews.Application.Contracts.ReaderPreferences;

/// <summary>
/// What: API contract for reader preferences.
/// How: Uses frontend-friendly primitive values for theme, font scale, compact cards, and categories.
/// Why: The frontend should not know about persistence records or future account/profile storage details.
/// </summary>
public sealed record ReaderPreferencesDto(
    string Theme,
    decimal FontScale,
    bool CompactCards,
    IReadOnlyList<string> PreferredCategories);
