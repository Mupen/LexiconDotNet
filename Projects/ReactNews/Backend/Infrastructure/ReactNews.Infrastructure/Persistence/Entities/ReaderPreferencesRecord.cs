namespace ReactNews.Infrastructure.Persistence.Entities;

/// <summary>
/// What: Database row for one reader's profile preferences.
/// How: Stores scalar preference values and preferred categories as a comma-separated string.
/// Why: Preferences belong to accounts so different readers can keep separate display/feed settings.
/// </summary>
public sealed class ReaderPreferencesRecord
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Theme { get; set; } = string.Empty;

    public decimal FontScale { get; set; }

    public bool CompactCards { get; set; }

    public string PreferredCategories { get; set; } = string.Empty;
}
