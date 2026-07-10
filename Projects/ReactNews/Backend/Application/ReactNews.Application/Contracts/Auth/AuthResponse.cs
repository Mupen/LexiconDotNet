namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Response contract returned after successful login/register/me operations.
/// How: Wraps the authenticated user DTO.
/// Why: A wrapper keeps auth responses extensible if session metadata is added later.
/// </summary>
public sealed record AuthResponse(AuthUserDto User);
