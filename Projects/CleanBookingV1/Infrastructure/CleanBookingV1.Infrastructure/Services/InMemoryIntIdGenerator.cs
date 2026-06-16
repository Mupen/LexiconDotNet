using CleanBookingV1.Application.Interfaces;

namespace CleanBookingV1.Infrastructure.Services;

public sealed class InMemoryIntIdGenerator : IIntIdGenerator
{
    private int _currentId;

    public InMemoryIntIdGenerator(int start = 0)
    {
        _currentId = start;
    }

    public int NextId()
    {
        _currentId++;
        return _currentId;
    }
}