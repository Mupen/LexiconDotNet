using ReactNews.Application.Interfaces;
using ReactNews.Application.Services.ReaderPreferences;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Persistence.Entities;
using ReaderPreferencesEntity = ReactNews.Domain.Entities.ReaderPreferences.ReaderPreferences;

namespace ReactNews.Infrastructure.Storage.ReaderPreferences;

/// <summary>
/// What: EF Core implementation for reader preferences.
/// How: Reads and writes one row per authenticated user id.
/// Why: Each account should own its own theme, font, compact-card, and category choices.
/// </summary>
public sealed class EfReaderPreferencesStore : IReaderPreferencesStore
{
    private readonly ReactNewsDbContext _dbContext;

    public EfReaderPreferencesStore(ReactNewsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// What: Gets saved preferences or creates default preferences for one user.
    /// How: Looks for the row keyed by user id and maps it; if missing, saves defaults and returns them.
    /// Why: The frontend can always call GET preferences and receive a complete usable object.
    /// </summary>
    public ReaderPreferencesEntity Get(string userId)
    {
        var record = _dbContext.ReaderPreferences.Find(userId);

        if (record is not null)
        {
            return ToDomain(record);
        }

        return Save(userId, ReaderPreferencesFactory.CreateDefault());
    }

    /// <summary>
    /// What: Saves the complete reader preference object for one user.
    /// How: Inserts or updates the user row and serializes categories into a simple string.
    /// Why: A full replace keeps persistence logic small while each account keeps separate preferences.
    /// </summary>
    public ReaderPreferencesEntity Save(string userId, ReaderPreferencesEntity preferences)
    {
        var record = _dbContext.ReaderPreferences.Find(userId);

        if (record is null)
        {
            record = new ReaderPreferencesRecord { Id = userId, UserId = userId };
            _dbContext.ReaderPreferences.Add(record);
        }

        record.UserId = userId;
        record.Theme = preferences.Theme;
        record.FontScale = preferences.FontScale;
        record.CompactCards = preferences.CompactCards;
        record.PreferredCategories = string.Join(',', preferences.PreferredCategories);

        _dbContext.SaveChanges();

        return ToDomain(record);
    }

    /// <summary>
    /// What: Converts an EF reader-preference row into the domain preference object.
    /// How: splits the comma-separated category string and copies theme, font
    /// scale, and compact-card settings into the domain record.
    /// Why: the domain model should expose categories as a list even though SQLite
    /// stores the compact representation as text.
    /// </summary>
    private static ReaderPreferencesEntity ToDomain(ReaderPreferencesRecord record)
    {
        var categories = record.PreferredCategories
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ReaderPreferencesEntity(
            Theme: record.Theme,
            FontScale: record.FontScale,
            CompactCards: record.CompactCards,
            PreferredCategories: categories);
    }
}
