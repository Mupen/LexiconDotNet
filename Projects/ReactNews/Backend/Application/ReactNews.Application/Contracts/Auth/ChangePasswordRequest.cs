namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Request contract for changing an authenticated user's password.
/// How: Sends both the current password and the new password.
/// Why: Requiring the current password prevents a left-open browser session from
/// silently taking over the account.
/// </summary>
public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
