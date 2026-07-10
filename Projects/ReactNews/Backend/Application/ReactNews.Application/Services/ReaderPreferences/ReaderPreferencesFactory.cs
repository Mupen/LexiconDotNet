using ReactNews.Application.Contracts.ReaderPreferences;
using ReaderPreferencesEntity = ReactNews.Domain.Entities.ReaderPreferences.ReaderPreferences;

namespace ReactNews.Application.Services.ReaderPreferences;

/// <summary>
/// What: Creates validated reader preference domain objects from API input.
/// How: Normalizes theme, clamps/validates font scale, applies compact-card defaults, and validates preferred categories.
/// Why: Preference rules belong in Application so API controllers and EF stores do not duplicate business validation.
/// </summary>
public static class ReaderPreferencesFactory
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "business",
        "entertainment",
        "general",
        "health",
        "science",
        "sports",
        "technology"
    };

    /// <summary>
    /// What: Creates default preferences for a new reader profile.
    /// How: Uses light theme, normal font scale, non-compact cards, and technology/general categories.
    /// Why: Defaults keep the UI useful before the reader customizes anything.
    /// </summary>
    public static ReaderPreferencesEntity CreateDefault()
    {
        return new ReaderPreferencesEntity(
            Theme: "light",
            FontScale: 1.0m,
            CompactCards: false,
            PreferredCategories: new[] { "technology", "general" });
    }

    /// <summary>
    /// What: Converts an update request into a validated preference object.
    /// How: Missing values fall back to defaults and supplied values are normalized.
    /// Why: The backend should protect persisted settings from invalid UI/manual HTTP input.
    /// </summary>
    public static ReaderPreferencesEntity Create(UpdateReaderPreferencesRequest request)
    {
        var defaults = CreateDefault();

        return new ReaderPreferencesEntity(
            Theme: NormalizeTheme(request.Theme ?? defaults.Theme),
            FontScale: NormalizeFontScale(request.FontScale ?? defaults.FontScale),
            CompactCards: request.CompactCards ?? defaults.CompactCards,
            PreferredCategories: NormalizeCategories(request.PreferredCategories ?? defaults.PreferredCategories));
    }

    /// <summary>
    /// What: Converts the submitted theme into the stored theme value.
    /// How: trims/lowercases the value and accepts only light or dark.
    /// Why: the frontend theme switch expects a small known set of values, so
    /// persistence should reject unknown theme names.
    /// </summary>
    private static string NormalizeTheme(string theme)
    {
        var normalized = theme.Trim().ToLowerInvariant();

        if (normalized is not "light" and not "dark")
        {
            throw new ArgumentException("Theme must be either light or dark.");
        }

        return normalized;
    }

    /// <summary>
    /// What: Validates and formats the reader's preferred font scale.
    /// How: enforces the supported range and rounds the value to two decimals.
    /// Why: font scaling should remain useful for accessibility without allowing
    /// extreme values that break article layouts.
    /// </summary>
    private static decimal NormalizeFontScale(decimal fontScale)
    {
        if (fontScale is < 0.85m or > 1.3m)
        {
            throw new ArgumentException("Font scale must be between 0.85 and 1.3.");
        }

        return decimal.Round(fontScale, 2);
    }

    /// <summary>
    /// What: Converts the submitted preferred-category list into stored category values.
    /// How: removes blank entries, trims/lowercases values, removes duplicates, and
    /// rejects categories outside the supported news category list.
    /// Why: the personal feed depends on clean category values and should not need
    /// to handle duplicate or misspelled preference data.
    /// </summary>
    private static IReadOnlyList<string> NormalizeCategories(IReadOnlyList<string> categories)
    {
        var normalized = categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException("At least one preferred category is required.");
        }

        var invalid = normalized.FirstOrDefault(category => !AllowedCategories.Contains(category));

        if (invalid is not null)
        {
            throw new ArgumentException("Preferred categories must be one of: business, entertainment, general, health, science, sports, technology.");
        }

        return normalized;
    }
}
