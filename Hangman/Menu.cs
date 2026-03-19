namespace Hangman
{
    /// <summary>
    /// Handles all menu screens in the game.
    /// Responsible for displaying the main menu and pause menu,
    /// and returning the player's chosen action to the caller.
    /// </summary>
    internal class Menu
    {
        /// <summary>
        /// Displays the main menu and handles navigation.
        /// This is the entry point of the game loop — the player can
        /// start a new game or quit from here.
        /// Loops indefinitely until the player chooses to quit.
        /// </summary>
        public static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("===================");
                Console.WriteLine("     HANGMAN       ");
                Console.WriteLine("===================\n");
                Console.WriteLine("1. New Game");
                Console.WriteLine("2. Quit");
                Console.WriteLine();
                Console.Write("Choose an option: ");
                string? input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        Game game = new();
                        game.Run();
                        break;
                    case "2":
                        Console.WriteLine("\nGoodbye!");
                        return;
                    default:
                        Console.WriteLine("\nInvalid option, press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the pause menu during an active game.
        /// Returns a MenuAction enum value so Game.cs knows
        /// what to do next (resume, start new game, or quit).
        /// </summary>
        /// <returns>MenuAction indicating the player's choice</returns>
        public static MenuAction ShowPauseMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("===================");
                Console.WriteLine("      PAUSED        ");
                Console.WriteLine("===================\n");
                Console.WriteLine("0. Resume");
                Console.WriteLine("1. New Game");
                Console.WriteLine("2. Quit");
                Console.WriteLine();
                Console.Write("Choose an option: ");
                string? input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "0": return MenuAction.Resume;
                    case "1": return MenuAction.NewGame;
                    case "2": return MenuAction.Quit;
                    default:
                        Console.WriteLine("Invalid option, press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Represents the possible actions a player can take from the pause menu.
    /// Returned by ShowPauseMenu() and handled in Game.cs.
    /// Resume  - continue the current game
    /// NewGame - abandon current game and start fresh
    /// Quit    - exit the application entirely
    /// </summary>
    internal enum MenuAction
    {
        Resume,
        NewGame,
        Quit
    }
}