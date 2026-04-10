using SalesSystem.Api.Interfaces;

namespace SalesSystem.UnitTests.Api.Fakes;

public sealed class FakeMenu : IMenu
{
    public int RunCallCount { get; private set; }

    public Task RunAsync()
    {
        RunCallCount++;
        return Task.CompletedTask;
    }
}