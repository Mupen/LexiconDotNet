namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Generates ids for new bookings.
/// Abstracting Guid creation makes tests deterministic while production code still
/// uses real random GUID values.
/// </summary>
public interface IGuidGenerator
{
    Guid NewGuid();
}
