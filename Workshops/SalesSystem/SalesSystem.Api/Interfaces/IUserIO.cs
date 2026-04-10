namespace SalesSystem.Api.Interfaces;

public interface IUserIO
{
    void Clear();
    void Write(string text);
    void WriteLine(string text = "");
    string? ReadLine();
    void WaitForKey();
}