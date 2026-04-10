using SalesSystem.Api.Helpers;
using SalesSystem.Api.Interfaces;

namespace SalesSystem.Api.UI;

public abstract class MenuBase : IMenu
{
    protected readonly IUserIO _ui;

    protected MenuBase(IUserIO ui)
    {
        _ui = ui;
    }

    public abstract Task RunAsync();

    // ---------- Common UI helpers ----------

    protected void ShowHeader(string title, params string[] messages)
    {
        _ui.Clear();

        const int totalWidth = 100;

        string content = $" {title} ";
        int remaining = totalWidth - content.Length;

        int left = remaining / 2;
        int right = remaining - left;

        string line = new string('=', left) + content + new string('=', right);

        _ui.WriteLine(line);

        foreach (var message in messages)
        {
            _ui.WriteLine(message);
        }
    }

    protected void ShowSubHeader(string title, params string[] messages)
    {
        const int totalWidth = 100;

        string content = $" {title} ";
        int remaining = totalWidth - content.Length;

        int left = remaining / 2;
        int right = remaining - left;

        string line = new string('=', left) + content + new string('=', right);

        _ui.WriteLine(line);

        foreach (var message in messages)
        {
            _ui.WriteLine(message);
        }
    }

    protected void ShowMessage(params string[] messages)
    {
        foreach (var message in messages)
        {
            _ui.WriteLine(message);
        }
    }

    protected void ShowPause(string message = "Press any key to continue...", string prompt = "> ")
    {
        _ui.WriteLine();
        _ui.WriteLine(message);
        _ui.WriteLine();
        _ui.Write(prompt);
        _ui.WaitForKey();
    }

    protected string ShowPrompt(string? message = null, string prompt = "> ")
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _ui.WriteLine();
            _ui.WriteLine(message);
            
        }

        _ui.WriteLine();
        _ui.Write(prompt);
        return _ui.ReadLine() ?? string.Empty;
    }

    // ---------- Input helpers ----------

    protected string ReadRequiredText(string label)
    {
        while (true)
        {
            _ui.Write(label);
            var input = _ui.ReadLine();

            if (!UserInput.IsEmpty(input ?? string.Empty))
                return input!.Trim();

            _ui.WriteLine("Value is required.");
        }
    }

    protected int ReadInt(string label)
    {
        while (true)
        {
            _ui.Write(label);
            var input = _ui.ReadLine();

            if (UserInput.TryParseInt(input ?? string.Empty, out int value))
                return value;

            _ui.WriteLine("Invalid number.");
        }
    }

    protected decimal ReadDecimal(string label)
    {
        while (true)
        {
            _ui.Write(label);
            var input = _ui.ReadLine();

            if (UserInput.TryParseDecimal(input ?? string.Empty, out decimal value))
                return value;

            _ui.WriteLine("Invalid decimal number.");
        }
    }

    protected bool ReadYesNo(string label)
    {
        while (true)
        {
            _ui.Write(label);
            var input = _ui.ReadLine() ?? string.Empty;

            if (UserInput.IsYes(input))
                return true;

            if (UserInput.IsNo(input))
                return false;

            _ui.WriteLine("Please enter Y or N.");
        }
    }

    protected decimal ReadVatRate(string label)
    {
        while (true)
        {
            decimal value = ReadDecimal(label);

            if (value >= 0 && value < 1)
                return value;

            ShowMessage("VAT must be less than 1. Example: 0,25 or 0.25");
        }
    }
}