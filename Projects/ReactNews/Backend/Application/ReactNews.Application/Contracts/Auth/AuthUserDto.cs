namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: API contract for the currently authenticated user.
/// How: Exposes only safe identity and role fields.
/// Why: Password hashes and persistence details must never be returned to the frontend.
/// </summary>
public sealed record AuthUserDto(
    string Id,
    string Email,
    string DisplayName,
    string Role);
