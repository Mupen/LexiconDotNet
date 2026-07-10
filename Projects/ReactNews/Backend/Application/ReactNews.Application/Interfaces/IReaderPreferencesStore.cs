using ReactNews.Domain.Entities.ReaderPreferences;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// What: Stores and retrieves one authenticated reader's preferences.
/// How: Application use cases depend on this interface while Infrastructure provides the EF Core implementation.
/// Why: Preferences are user-facing state, but database details should stay outside Application.
/// </summary>
public interface IReaderPreferencesStore
{
    /// <summary>
    /// What: Gets the saved preferences or creates defaults when no row exists for the user.
    /// How: The concrete store reads preferences by user id.
    /// Why: Preferences should belong to the signed-in account so users do not share theme/feed settings.
    /// </summary>
    ReaderPreferences Get(string userId);

    /// <summary>
    /// What: Saves the full preference object for one user.
    /// How: The concrete store inserts or updates the row keyed by user id.
    /// Why: Replacing the full object keeps preference updates predictable and keeps each account isolated.
    /// </summary>
    ReaderPreferences Save(string userId, ReaderPreferences preferences);
}
