using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.Auth;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.Users;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Creates a new ReactNews account.
/// How: Validates input, hashes the password, saves the user, and returns a safe auth response.
/// Why: Registration is a business use case and should be testable without HTTP cookie code.
/// </summary>
public sealed class RegisterUser
{
    private readonly IUserStore _userStore;

    public RegisterUser(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public Result<AuthResponse> Execute(RegisterRequest request)
    {
        try
        {
            var email = NormalizeEmail(request.Email);

            if (_userStore.FindByEmail(email) is not null)
            {
                return Result<AuthResponse>.Failure(Error.Validation("Email is already registered."));
            }

            var password = request.Password?.Trim() ?? string.Empty;

            if (password.Length < 8)
            {
                return Result<AuthResponse>.Failure(Error.Validation("Password must be at least 8 characters."));
            }

            var user = new User(
                Id: Guid.NewGuid().ToString("N"),
                Email: email,
                DisplayName: NormalizeDisplayName(request.DisplayName),
                Role: UserRole.Reader,
                PasswordHash: PasswordHasher.Hash(password),
                CreatedAtUtc: DateTimeOffset.UtcNow);

            return Result<AuthResponse>.Success(new AuthResponse(_userStore.Save(user).ToDto()));
        }
        catch (ArgumentException ex)
        {
            return Result<AuthResponse>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// What: Converts a submitted email address into the stored canonical form.
    /// How: trims whitespace, lowercases the value, and rejects values that are too
    /// short or do not contain an at-sign.
    /// Why: account lookup should be case-insensitive and consistent, while invalid
    /// email-like input should fail before a user entity is created.
    /// </summary>
    private static string NormalizeEmail(string? email)
    {
        var normalized = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (normalized.Length < 5 || !normalized.Contains('@'))
        {
            throw new ArgumentException("A valid email address is required.");
        }

        return normalized;
    }

    /// <summary>
    /// What: Converts a submitted display name into the stored display value.
    /// How: trims surrounding whitespace and enforces the allowed length range.
    /// Why: display names are shown in the UI, so they should not be empty,
    /// extremely short, or large enough to damage layout/readability.
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
