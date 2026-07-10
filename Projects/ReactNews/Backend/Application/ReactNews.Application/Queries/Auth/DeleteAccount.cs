using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Services.Auth;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Deletes the signed-in user's account.
/// How: Verifies the current password and then asks IUserStore to delete the
/// user by id.
/// Why: Deletion is destructive account logic, so the Application layer should
/// own the password confirmation and not leave it to the controller.
/// </summary>
public sealed class DeleteAccount
{
    private readonly IUserStore _userStore;

    public DeleteAccount(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// What: Executes account deletion for one authenticated user.
    /// How: Finds the user, verifies CurrentPassword against the stored hash,
    /// and deletes the user record.
    /// Why: Returning Result keeps expected failures such as wrong password as
    /// clean HTTP 400 responses instead of exceptions.
    /// </summary>
    public Result<bool> Execute(string userId, DeleteAccountRequest request)
    {
        var user = _userStore.FindById(userId);

        if (user is null)
        {
            return Result<bool>.Failure(Error.NotFound("Account was not found."));
        }

        var currentPassword = request.CurrentPassword?.Trim() ?? string.Empty;

        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
        {
            return Result<bool>.Failure(Error.Validation("Current password is incorrect."));
        }

        return _userStore.Delete(userId)
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(Error.NotFound("Account was not found."));
    }
}
