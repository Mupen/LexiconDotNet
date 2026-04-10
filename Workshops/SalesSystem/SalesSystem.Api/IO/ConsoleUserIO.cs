using SalesSystem.Api.Interfaces;

namespace SalesSystem.Api.IO;

public class ConsoleUserIO : IUserIO
{
    public void Clear() => Console.Clear();

    public void Write(string text) => Console.Write(text);

    public void WriteLine(string text = "")
        => Console.WriteLine(text);

    public string? ReadLine() => Console.ReadLine();

    public void WaitForKey() => Console.ReadKey();
}