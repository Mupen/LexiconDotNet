namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Request contract for changing account profile fields.
/// How: Currently carries only DisplayName because email changes need extra
/// verification rules that are outside the current account-management scope.
/// Why: A separate contract lets profile edits evolve without changing login or
/// registration request shapes.
/// </summary>
public sealed record UpdateProfileRequest(string? DisplayName);
