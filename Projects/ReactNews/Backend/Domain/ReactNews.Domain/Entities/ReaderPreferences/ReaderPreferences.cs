namespace ReactNews.Domain.Entities.ReaderPreferences;

/// <summary>
/// What: Represents one reader's display and feed preferences.
/// How: Stores theme, font scale, compact-card mode, and preferred news categories together.
/// Why: These values describe reader intent and should later attach naturally to a real user account.
/// </summary>
public sealed record ReaderPreferences(
    string Theme,
    decimal FontScale,
    bool CompactCards,
    IReadOnlyList<string> PreferredCategories);
