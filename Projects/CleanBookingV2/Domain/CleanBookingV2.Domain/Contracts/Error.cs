namespace CleanBookingV2.Domain.Contracts;

/// <summary>
/// Describes an expected business or validation failure.
/// The Code is stable and machine-readable for API mapping, while Message is aimed
/// at humans. This is clearer than returning plain strings and avoids using
/// exceptions for normal business outcomes such as unavailable rooms.
/// </summary>
public sealed record Error(string Code, string Message);
