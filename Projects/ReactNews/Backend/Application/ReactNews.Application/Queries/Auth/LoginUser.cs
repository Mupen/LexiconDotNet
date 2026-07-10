using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Services.Auth;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Validates login credentials for an existing user.
/// How: Looks up by email, verifies the password hash, and returns a safe auth response.
/// Why: Login should be testable application logic; the API layer only creates the cookie after success.
/// </summary>
public sealed class LoginUser
{
    private readonly IUserStore _userStore;

    public LoginUser(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public Result<AuthResponse> Execute(LoginRequest request)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = request.Password?.Trim() ?? string.Empty;
        var user = _userStore.FindByEmail(email);

        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(Error.Validation("Email or password is incorrect."));
        }

        return Result<AuthResponse>.Success(new AuthResponse(user.ToDto()));
    }
}
