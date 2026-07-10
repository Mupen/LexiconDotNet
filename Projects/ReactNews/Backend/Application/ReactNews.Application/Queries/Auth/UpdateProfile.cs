using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Updates profile information for the signed-in account.
/// How: Loads the user by id, validates the new display name, saves a copied
/// User record with the changed value, and returns the safe auth DTO.
/// Why: Profile editing is account business logic and should be testable without
/// ASP.NET cookies or EF Core.
/// </summary>
public sealed class UpdateProfile
{
    private readonly IUserStore _userStore;

    public UpdateProfile(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// What: Executes a display-name update.
    /// How: Uses the immutable record `with` expression to create the changed
    /// user while preserving id, email, role, password hash, and created date.
    /// Why: Updating only the intended field avoids accidental role/email changes
    /// from a profile form.
    /// </summary>
    public Result<AuthResponse> Execute(string userId, UpdateProfileRequest request)
    {
        try
        {
            var user = _userStore.FindById(userId);

            if (user is null)
            {
                return Result<AuthResponse>.Failure(Error.NotFound("Account was not found."));
            }

            var updated = user with { DisplayName = NormalizeDisplayName(request.DisplayName) };
            return Result<AuthResponse>.Success(new AuthResponse(_userStore.Save(updated).ToDto()));
        }
        catch (ArgumentException ex)
        {
            return Result<AuthResponse>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// What: Converts a submitted display name into the stored profile value.
    /// How: trims surrounding whitespace and validates the minimum and maximum
    /// length accepted by the account model.
    /// Why: profile updates should follow the same display-name rules as
    /// registration so accounts remain consistent over time.
    /// </summary>
    private static string NormalizeDisplayName(string? displayName)
    {
        var normalized = displayName?.Trim() ?? string.Empty;

        if (normalized.Length is < 2 or > 80)
        {
            throw new ArgumentException("Display name must be between 2 and 80 characters.");
        }

        return normalized;
    }
}
