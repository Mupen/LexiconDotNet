namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Request contract for signing in.
/// How: Carries email and password from the frontend login form.
/// Why: Controllers should bind raw input and let Application validate credentials.
/// </summary>
public sealed record LoginRequest(string? Email, string? Password);
