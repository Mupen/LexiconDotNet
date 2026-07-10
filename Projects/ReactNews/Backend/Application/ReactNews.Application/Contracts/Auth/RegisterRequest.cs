namespace ReactNews.Application.Contracts.Auth;

/// <summary>
/// What: Request contract for creating a ReactNews account.
/// How: Carries email, display name, password, and an ignored legacy role field.
/// Why: Public registration always creates Reader accounts; Admin accounts are
/// created through controlled seed configuration instead of user-submitted role text.
/// </summary>
public sealed record RegisterRequest(
    string? Email,
    string? DisplayName,
    string? Password,
    string? Role);
