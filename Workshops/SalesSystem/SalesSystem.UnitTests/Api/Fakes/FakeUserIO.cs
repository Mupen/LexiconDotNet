using SalesSystem.Api.Interfaces;
using System.Text;

namespace SalesSystem.UnitTests.Api.Fakes;

public sealed class FakeUserIO : IUserIO
{
    private readonly Queue<string?> _inputs = new();
    private readonly List<string> _outputs = new();

    public int ClearCallCount { get; private set; }
    public int WaitCallCount { get; private set; }

    public IReadOnlyList<string> Outputs => _outputs;

    public void AddInput(string? input)
    {
        _inputs.Enqueue(input);
    }

    public void AddInputs(params string?[] inputs)
    {
        foreach (var input in inputs)
        {
            _inputs.Enqueue(input);
        }
    }

    public void Clear()
    {
        ClearCallCount++;
    }

    public void Write(string text)
    {
        _outputs.Add(text);
    }

    public void WriteLine(string text = "")
    {
        _outputs.Add(text);
    }

    public string? ReadLine()
    {
        if (_inputs.Count == 0)
        {
            throw new InvalidOperationException(
                "FakeUserIO has no more queued inputs.");
        }

        return _inputs.Dequeue();
    }

    public void WaitForKey()
    {
        WaitCallCount++;
    }

    public string GetAllOutput()
    {
        return string.Join(Environment.NewLine, _outputs);
    }

    public bool ContainsOutput(string expectedText)
    {
        return _outputs.Any(x => x.Contains(expectedText, StringComparison.Ordinal));
    }
}