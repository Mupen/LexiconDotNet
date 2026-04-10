namespace SalesSystem.Api.Helpers;

internal static class UserInput
{
    // Normalize raw input (trim + uppercase)
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return input.Trim().ToUpperInvariant();
    }

    // Check if input matches a command (e.g., "X", "H", etc.)
    public static bool IsCommand(string input, params string[] commands)
    {
        var normalized = Normalize(input);
        return commands.Contains(normalized);
    }

    // Check if input is empty after normalization
    public static bool IsEmpty(string input)
    {
        return string.IsNullOrEmpty(Normalize(input));
    }

    // Try parse integer safely
    public static bool TryParseInt(string input, out int value)
    {
        return int.TryParse(input, out value);
    }

    // Try parse decimal safely (for price, etc.)
    public static bool TryParseDecimal(string input, out decimal value)
    {
        input = input.Replace(',', '.');

        return decimal.TryParse(
            input,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    // Placeholder for future: parse complex commands (e.g. "2:1,3:2")
    public static bool TryParseCommand(string input, out object? result)
    {
        result = null;

        // TODO: implement complex command parsing
        return false;
    }

    // Placeholder for future: yes/no confirmation
    public static bool IsYes(string input)
    {
        var normalized = Normalize(input);
        return normalized == "Y" || normalized == "YES";
    }

    public static bool IsNo(string input)
    {
        var normalized = Normalize(input);
        return normalized == "N" || normalized == "NO";
    }
}