using CleanBookingV1.Application.Interfaces;

namespace CleanBookingV1.UnitTests.Helpers.TestDoubles.Stubs;

public sealed class FixedGuidGenerator : IGuidGenerator
{
    private readonly Guid _guid;

    public FixedGuidGenerator(Guid guid)
    {
        _guid = guid;
    }

    public Guid NewGuid()
    {
        return _guid;
    }
}