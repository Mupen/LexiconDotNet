namespace ReactNews.Api.Contracts.Common;

/// <summary>
/// Standard error response shape returned by the API for expected failures.
/// </summary>
/// <remarks>
/// What: exposes a machine-readable code and a readable message.
/// Why: React can show the message to users, while tests or future clients can
/// assert on the code without parsing free-form text.
/// How: ApiResultMapping creates this response from application Error values.
/// </remarks>
public sealed record ErrorResponse(string Code, string Error);
