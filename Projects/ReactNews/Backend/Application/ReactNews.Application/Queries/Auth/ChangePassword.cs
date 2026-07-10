using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.Auth;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Changes the signed-in user's password.
/// How: Verifies the current password, validates the new password, hashes it,
/// saves the user, and returns the current safe auth DTO.
/// Why: Password storage rules belong in Application so controllers do not touch
/// password hashes directly.
/// </summary>
public sealed class ChangePassword
{
    private readonly IUserStore _userStore;

    public ChangePassword(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// What: Executes a password change for one account id.
    /// How: Reads the persisted hash, verifies CurrentPassword, hashes
    /// NewPassword, and saves the updated User record.
    /// Why: A signed-in cookie alone should not be enough to change credentials.
    /// </summary>
    public Result<AuthResponse> Execute(string userId, ChangePasswordRequest request)
    {
        var user = _userStore.FindById(userId);

        if (user is null)
        {
            return Result<AuthResponse>.Failure(Error.NotFound("Account was not found."));
        }

        var currentPassword = request.CurrentPassword?.Trim() ?? string.Empty;
        var newPassword = request.NewPassword?.Trim() ?? string.Empty;

        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(Error.Validation("Current password is incorrect."));
        }

        if (newPassword.Length < 8)
        {
            return Result<AuthResponse>.Failure(Error.Validation("New password must be at least 8 characters."));
        }

        var updated = user with { PasswordHash = PasswordHasher.Hash(newPassword) };
        return Result<AuthResponse>.Success(new AuthResponse(_userStore.Save(updated).ToDto()));
    }
}
