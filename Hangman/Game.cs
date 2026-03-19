using System;
using System.Collections.Generic;

namespace Hangman
{
    /// <summary>
    /// Core game logic for Hangman.
    /// One word per game session — player wins or loses, then returns to main menu.
    /// Communicates with Menu.cs for pause functionality.
    /// </summary>
    internal class Game
    {
        // ─── Session State ─────────────────────────────────────────────

        /// <summary>
        /// Total guesses remaining for the game.
        /// Player loses when this hits 0.
        /// </summary>
        private int guessesRemaining = 10;

        /// <summary>
        /// Stores the last feedback message to display on next redraw.
        /// Set by guess handlers, displayed in DisplayState() as the Status line.
        /// Decouples guess results from the display cycle — handlers don't need
        /// to know when or how the screen is drawn, they just set a message.
        /// </summary>
        private string lastMessage = string.Empty;

        // ─── Current Word State ─────────────────────────────────────────

        /// <summary>
        /// The secret word the player is trying to guess.
        /// Always lowercase to match player input.
        /// </summary>
        private string secretWord = string.Empty;

        /// <summary>
        /// Represents the current revealed state of the word.
        /// Unrevealed letters are stored as '_'.
        /// e.g. for "hangman" → ['_','_','_','_','_','_','_']
        /// Correct guesses fill in the corresponding positions.
        /// </summary>
        private char[] revealedWord = [];

        /// <summary>
        /// Tracks incorrect letter guesses.
        /// Displayed to the player after each guess.
        /// </summary>
        private readonly List<char> incorrectGuesses = [];

        /// <summary>
        /// Tracks all letters guessed (correct and incorrect).
        /// Used to detect duplicate guesses so they don't consume a guess.
        /// </summary>
        private readonly List<char> allGuessedLetters = [];

        /// <summary>
        /// Tracks all full word guesses.
        /// Used to detect duplicate word guesses so they don't consume a guess.
        /// </summary>
        private readonly List<string> allGuessedWords = [];

        // ─── Entry Point ────────────────────────────────────────────────

        /// <summary>
        /// Main entry point called by Menu.cs.
        /// Owns the restart loop so new games reuse the same Game instance
        /// Resets all session state between rounds.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                LoadWord();
                bool startNewGame = GameLoop();
                if (!startNewGame) break;

                // Reset all session state for the next game
                guessesRemaining = 10;
                lastMessage = string.Empty;
                incorrectGuesses.Clear();
                allGuessedLetters.Clear();
                allGuessedWords.Clear();
            }
        }

        // ─── Game Loop ──────────────────────────────────────────────────

        /// <summary>
        /// Core game loop. Runs until the player wins, loses, or requests a menu action.
        /// Returns true if the player chose New Game from the pause menu,
        /// signalling Run() to reset state and start a fresh round.
        /// Returns false if the game ended normally (win/loss) or the player quit.
        /// </summary>
        private bool GameLoop()
        {
            while (guessesRemaining > 0)
            {
                Console.Clear(); Console.WriteLine("\x1b[3J");
                DisplayState();

                Console.Write("Enter a letter or guess the full word (0 = menu): ");
                string? input = Console.ReadLine()?.Trim().ToLower();

                // Guard against empty input
                if (string.IsNullOrEmpty(input))
                {
                    lastMessage = "Invalid input, try again...";
                    continue;
                }

                // Player wants to open pause menu
                if (input == "0")
                {
                    MenuAction action = Menu.ShowPauseMenu();

                    switch (action)
                    {
                        case MenuAction.Resume:
                            continue;
                        case MenuAction.NewGame:
                            return true;
                        case MenuAction.Quit:
                            Console.WriteLine("\nGoodbye!");
                            Environment.Exit(0);
                            return false;
                    }
                }

                // Single character = letter guess
                if (input.Length == 1 && char.IsLetter(input[0]))
                {
                    GuessLetter(input[0]);
                }
                // Multiple characters = full word guess
                else if (input.Length > 1)
                {
                    GuessWord(input);
                }
                else
                {
                    lastMessage = "Invalid input, try again...";
                }

                // Check if the player has revealed the full word
                if (IsWordSolved())
                {
                    OnWin();
                    return false;
                }
            }

            // No guesses remaining — game over
            OnGameOver();
            return false;
        }

        // ─── Guess Handlers ─────────────────────────────────────────────

        /// <summary>
        /// Handles a single letter guess.
        /// Duplicate guesses are flagged and don't consume a guess.
        /// Correct guesses reveal matching positions in the word.
        /// Incorrect guesses decrement guessesRemaining.
        /// Sets lastMessage instead of printing directly to avoid console bleeding.
        /// </summary>
        /// <param name="letter">The letter the player guessed</param>
        private void GuessLetter(char letter)
        {
            // Check for duplicate guess — don't consume a guess
            if (allGuessedLetters.Contains(letter))
            {
                lastMessage = $"You already guessed '{letter}'!";
                return;
            }

            // Register this letter as guessed
            allGuessedLetters.Add(letter);

            if (secretWord.Contains(letter))
            {
                // Reveal all positions in the word that match this letter
                for (int i = 0; i < secretWord.Length; i++)
                {
                    if (secretWord[i] == letter)
                        revealedWord[i] = letter;
                }
                lastMessage = $"Good guess! '{letter}' is in the word.";
            }
            else
            {
                // Wrong guess — track it and consume a guess
                incorrectGuesses.Add(letter);
                guessesRemaining--;
                lastMessage = $"Wrong! '{letter}' is not in the word.";
            }
        }

        /// <summary>
        /// Handles a full word guess.
        /// Correct guess reveals the entire word and triggers a win.
        /// Incorrect guess consumes a guess and reveals nothing.
        /// Sets lastMessage instead of printing directly to avoid console bleeding.
        /// </summary>
        /// <param name="word">The full word the player guessed</param>
        private void GuessWord(string word)
        {
            if (allGuessedWords.Contains(word))
            {
                lastMessage = $"You already guessed '{word}'!";
                return;
            }

            allGuessedWords.Add(word);

            if (word == secretWord)
            {
                for (int i = 0; i < secretWord.Length; i++)
                    revealedWord[i] = secretWord[i];

                lastMessage = "Correct! You guessed the word!";
            }
            else
            {
                guessesRemaining--;
                lastMessage = $"Wrong! '{word}' is not the word.";
            }
        }

        // ─── Win / Lose ──────────────────────────────────────────────────

        /// <summary>
        /// Called when the player successfully reveals the full word.
        /// Displays a win message then returns to main menu.
        /// </summary>
        private void OnWin()
        {
            Console.Clear(); Console.WriteLine("\x1b[3J");
            DisplayHangman();
            Console.WriteLine("You Won... The man is saved!");
            Console.WriteLine($"The word was: {secretWord.ToUpper()}");
            Console.Write("Press any key to return to menu:");
            Console.ReadKey();
        }

        /// <summary>
        /// Called when guessesRemaining hits 0.
        /// Reveals the secret word and displays a game over message.
        /// Then returns to the main menu.
        /// </summary>
        private void OnGameOver()
        {
            Console.Clear(); Console.WriteLine("\x1b[3J");
            DisplayHangman();
            Console.WriteLine("Game Over... The man is dead!");
            Console.WriteLine($"The word was: {secretWord.ToUpper()}");
            Console.Write("Press any key to return to menu:");
            Console.ReadKey();
        }

        // ─── Word Loading ────────────────────────────────────────────────

        /// <summary>
        /// Loads a random secret word from WordBank.
        /// Initializes the revealed word array with underscores.
        /// </summary>
        private void LoadWord()
        {
            secretWord = WordBank.GetRandomWord();
            revealedWord = new char[secretWord.Length];
            for (int i = 0; i < secretWord.Length; i++)
                revealedWord[i] = '_';
        }

        // ─── Win Check ───────────────────────────────────────────────────

        /// <summary>
        /// Returns true if all letters in the word have been revealed.
        /// Checks whether any '_' characters remain in revealedWord.
        /// </summary>
        private bool IsWordSolved()
        {
            foreach (char c in revealedWord)
                if (c == '_') return false;
            return true;
        }

        // ─── Display ─────────────────────────────────────────────────────

        /// <summary>
        /// Displays the current game state:
        /// </summary>
        private void DisplayState()
        {
            DisplayHangman();

            // ─── Guess Status ─────────────────────────────────────────────
            string status = !string.IsNullOrEmpty(lastMessage) ? lastMessage : "No guesses yet.";
            Console.WriteLine($"Status: {status}");

            // ─── Guesses Remaining ───────────────────────────────────────
            Console.WriteLine($"Guesses Remaining: {guessesRemaining}");

            // ─── Guessed Words ───────────────────────────────────────────
            string guessedWords = allGuessedWords.Count > 0 ? string.Join(", ", allGuessedWords) : "-";
            Console.WriteLine($"Guessed Words: {guessedWords}");

            // ─── Guessed Letters ─────────────────────────────────────────
            string guessedLetters = incorrectGuesses.Count > 0 ? string.Join(", ", incorrectGuesses) : "-";
            Console.WriteLine($"Guessed Letters: {guessedLetters}");

            // ─── Word ─────────────────────────────────────────────────────
            Console.WriteLine("Word: " + string.Join(" ", revealedWord));
        }

        /// <summary>
        /// Retrieves and displays the hangman art stage
        /// matching the current number of wrong guesses.
        /// Art stages are stored in Art.cs.
        /// </summary>
        private void DisplayHangman()
        {
            int wrongGuesses = 10 - guessesRemaining;
            Console.WriteLine(Art.Stages[wrongGuesses]);
        }
    }
}