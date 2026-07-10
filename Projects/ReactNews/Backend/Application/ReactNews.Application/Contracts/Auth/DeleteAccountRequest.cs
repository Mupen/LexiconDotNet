namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Request contract for deleting the current account.
/// How: Carries the current password as confirmation.
/// Why: Account deletion is destructive, so the backend should verify that the
/// current user knows the password before removing the account.
/// </summary>
public sealed record DeleteAccountRequest(string? CurrentPassword);
