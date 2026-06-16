namespace CleanBookingV1.Domain.Contracts;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}