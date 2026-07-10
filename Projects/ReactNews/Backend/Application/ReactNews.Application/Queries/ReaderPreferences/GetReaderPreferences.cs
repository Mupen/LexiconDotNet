using ReactNews.Application.Contracts.ReaderPreferences;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.ReaderPreferences;

/// <summary>
/// What: Loads the current authenticated reader's preferences.
/// How: Reads preferences through IReaderPreferencesStore by user id and maps them to a DTO.
/// Why: Controllers should expose preferences over HTTP without knowing persistence details.
/// </summary>
public sealed class GetReaderPreferences
{
    private readonly IReaderPreferencesStore _readerPreferencesStore;

    public GetReaderPreferences(IReaderPreferencesStore readerPreferencesStore)
    {
        _readerPreferencesStore = readerPreferencesStore;
    }

    public ReaderPreferencesDto Execute(string userId)
    {
        return _readerPreferencesStore.Get(userId).ToDto();
    }
}
