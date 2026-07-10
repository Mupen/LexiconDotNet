using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Queries.Auth;

namespace ReactNews.Api.Controllers.Auth;

/// <summary>
/// What: HTTP endpoints for registering, logging in, logging out, and reading the current account.
/// How: Application validates users and the controller issues/removes cookie authentication sessions.
/// Why: Authentication belongs at the API edge while account validation remains testable in Application.
/// </summary>
[ApiController]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// What: Creates a new reader account and starts an authenticated browser session.
    /// How: The request body is passed to RegisterUser; when registration succeeds,
    /// the returned user DTO is converted into authentication claims and written to
    /// the cookie authentication handler.
    /// Why: registration should leave the user signed in immediately, while account
    /// validation and duplicate-email rules remain inside the Application layer.
    /// </summary>
    [HttpPost("/api/auth/register")]
    public async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] RegisterUser useCase)
    {
        var result = useCase.Execute(request);

        if (result.IsSuccess && result.Value is not null)
        {
            await SignIn(result.Value.User);
        }

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Authenticates an existing account and starts a browser session.
    /// How: LoginUser checks the submitted credentials, then the controller signs in
    /// the returned user by issuing the configured authentication cookie.
    /// Why: credential checking belongs in Application/Infrastructure, while writing
    /// the HTTP cookie belongs at the API boundary where HttpContext is available.
    /// </summary>
    [HttpPost("/api/auth/login")]
    public async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginUser useCase)
    {
        var result = useCase.Execute(request);

        if (result.IsSuccess && result.Value is not null)
        {
            await SignIn(result.Value.User);
        }

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Ends the current authenticated browser session.
    /// How: ASP.NET removes the cookie authentication ticket for the configured
    /// cookie scheme and the endpoint returns a small success payload.
    /// Why: logout should invalidate the server-recognized session state instead of
    /// relying on the frontend to forget account information.
    /// </summary>
    [Authorize]
    [HttpPost("/api/auth/logout")]
    public async Task<IResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok(new { signedOut = true });
    }

    /// <summary>
    /// What: Returns the current signed-in user's account summary.
    /// How: The user id claim from the authentication cookie is passed to
    /// GetCurrentUser, which reads the current account data from persistence.
    /// Why: the frontend needs a trusted endpoint for restoring session state after
    /// page refreshes without storing the full account in browser storage.
    /// </summary>
    [Authorize]
    [HttpGet("/api/auth/me")]
    public IResult Me([FromServices] GetCurrentUser useCase)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var response = useCase.Execute(userId);
        return response is null ? Results.Unauthorized() : Results.Ok(response);
    }

    /// <summary>
    /// What: Updates the signed-in user's display/profile information.
    /// How: The user id is read from claims, the request is validated by
    /// UpdateProfile, and a successful update refreshes the auth cookie with the
    /// latest display-name claim.
    /// Why: claims can become stale after profile changes, so successful profile
    /// updates should rewrite the cookie before the next request.
    /// </summary>
    [Authorize]
    [HttpPut("/api/auth/profile")]
    public async Task<IResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        [FromServices] UpdateProfile useCase)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var result = useCase.Execute(userId, request);

        if (result.IsSuccess && result.Value is not null)
        {
            await SignIn(result.Value.User);
        }

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Changes the signed-in user's password.
    /// How: The authenticated user id and password request are forwarded to
    /// ChangePassword, which verifies the current password and stores the new hash.
    /// Why: password changes are account business rules and should not expose hash
    /// handling or user storage details to the controller.
    /// </summary>
    [Authorize]
    [HttpPut("/api/auth/password")]
    public IResult ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] ChangePassword useCase)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var result = useCase.Execute(userId, request);
        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Deletes the signed-in user's account when the confirmation is valid.
    /// How: DeleteAccount validates the request and removes related persisted data;
    /// on success the controller signs out the cookie before returning success.
    /// Why: a deleted account must not keep an active authentication cookie that
    /// refers to an account row that no longer exists.
    /// </summary>
    [Authorize]
    [HttpDelete("/api/auth/account")]
    public async Task<IResult> DeleteAccount(
        [FromBody] DeleteAccountRequest request,
        [FromServices] DeleteAccount useCase)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var result = useCase.Execute(userId, request);

        if (result.IsSuccess)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { deleted = true });
        }

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Returns the access-denied response used by cookie authorization.
    /// How: the cookie middleware redirects denied requests to this path, and this
    /// action converts that flow into a normal 403 response.
    /// Why: API clients should receive HTTP status codes instead of HTML login or
    /// access-denied pages.
    /// </summary>
    [HttpGet("/api/auth/denied")]
    public IResult Denied()
    {
        return Results.Forbid();
    }

    /// <summary>
    /// What: Writes the authenticated user's identity into the ASP.NET cookie scheme.
    /// How: account fields become standard claims, the claims are wrapped in a
    /// ClaimsPrincipal, and SignInAsync serializes that principal into the auth cookie.
    /// Why: standard claims let authorization policies and role checks work without
    /// each endpoint manually loading the user before checking permissions.
    /// </summary>
    private Task SignIn(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
