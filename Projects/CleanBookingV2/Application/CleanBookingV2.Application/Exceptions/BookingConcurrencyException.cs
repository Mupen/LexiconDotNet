namespace CleanBookingV2.Application.Exceptions;

/// <summary>
/// Represents an EF Core optimistic concurrency conflict in booking workflows.
/// Infrastructure throws this application-specific exception so use cases do not
/// depend on EF Core exception types. The use cases then translate it into Result
/// failures that the API maps to HTTP 409.
/// </summary>
public sealed class BookingConcurrencyException : Exception
{
    public BookingConcurrencyException()
        : base("The booking changed while this request was being processed.")
    {
    }
}
