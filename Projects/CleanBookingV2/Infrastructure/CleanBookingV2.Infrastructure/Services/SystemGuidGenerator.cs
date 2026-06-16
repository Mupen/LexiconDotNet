using CleanBookingV2.Application.Interfaces;

namespace CleanBookingV2.Infrastructure.Services;

/// <summary>
/// Production GUID generator implementation.
/// The application depends on IGuidGenerator so tests can provide deterministic ids,
/// while real infrastructure uses Guid.NewGuid for unique booking identities.
/// </summary>
public sealed class SystemGuidGenerator : IGuidGenerator
{
    /// <summary>
    /// Creates a new random GUID.
    /// This is intentionally tiny, but the abstraction makes use cases easier to
    /// test without relying on unpredictable values.
    /// </summary>
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}
