using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.ReaderPreferences;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.ReaderPreferences;

namespace ReactNews.Application.Queries.ReaderPreferences;

/// <summary>
/// What: Replaces the current authenticated reader's preferences.
/// How: Validates the request, saves the domain preference object, and maps the saved value back to a DTO.
/// Why: Preference validation should be explicit and testable before data reaches EF Core.
/// </summary>
public sealed class UpdateReaderPreferences
{
    private readonly IReaderPreferencesStore _readerPreferencesStore;

    public UpdateReaderPreferences(IReaderPreferencesStore readerPreferencesStore)
    {
        _readerPreferencesStore = readerPreferencesStore;
    }

    public Result<ReaderPreferencesDto> Execute(string userId, UpdateReaderPreferencesRequest request)
    {
        try
        {
            var preferences = ReaderPreferencesFactory.Create(request);
            var saved = _readerPreferencesStore.Save(userId, preferences);
            return Result<ReaderPreferencesDto>.Success(saved.ToDto());
        }
        catch (ArgumentException ex)
        {
            return Result<ReaderPreferencesDto>.Failure(Error.Validation(ex.Message));
        }
    }
}
