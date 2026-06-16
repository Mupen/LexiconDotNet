using CleanBookingV1.Application.Interfaces;

namespace CleanBookingV1.Infrastructure.Services;

public sealed class SystemGuidGenerator : IGuidGenerator
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}