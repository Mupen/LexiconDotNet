namespace Calculator.Core
{
    /// <summary>
    /// Entry point for the Calculator application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Creates a CalculatorMenu instance and starts the main program loop.
        /// </summary>
        static void Main()
        {
            CalculatorMenu menu = new();
            menu.Run();
        }
    }
}