using System.Globalization;

namespace Calculator.Core
{
    /// <summary>
    /// Controls the main program loop.
    /// Handles user input and routes commands or calculations.
    /// </summary>
    public class CalculatorMenu
    {
        private readonly CalculatorCalculation _calculation = new();
        private readonly CalculatorDisplay _display = new();

        /// <summary>
        /// Reads user input from console.
        /// </summary>
        private string? ReadInput()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Starts the calculator loop until user exits.
        /// </summary>
        public void Run()
        {
            _display.WelcomeMessage();

            while (true)
            {
                int lineBeforeInput = Console.CursorTop;
                string? input = ReadInput();

                if (input == null)
                    break;

                switch (input.Trim().ToLower())
                {
                    case "exit":
                        _display.ExitCommand();
                        return;

                    case "help":
                        _display.HelpCommand(lineBeforeInput);
                        continue;

                    case "clear":
                        _display.ClearCommand();
                        _calculation.ClearReferences();
                        continue;

                    default:
                        try
                        {
                            // Evaluate user expression
                            double result = _calculation.CalculateExpression(input);

                            // Display result and store history
                            _display.UpdateHistory(input, result.ToString("G15", CultureInfo.InvariantCulture));
                        }
                        catch (Exception ex)
                        {
                            // Show error without breaking loop
                            _display.ErrorCommand(ex.Message, lineBeforeInput);
                        }
                        break;
                }
            }
        }
    }
}