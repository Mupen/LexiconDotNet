namespace Hangman
{
    /// <summary>
    /// Responsible for storing the pool of possible secret words
    /// and selecting one at random for each new game.
    /// Keeping this separate from Game.cs makes it easy to
    /// add or change words without touching any game logic.
    /// </summary>
    internal class WordBank
    {
        /// <summary>
        /// Shared Random instance used for selecting a random word.
        /// Declared static so only one instance exists for the lifetime of the class,
        /// rather than creating a new one on every GetRandomWord() call.
        /// Declared readonly so it cannot be reassigned after initialization.
        /// A single long-lived instance also avoids the risk of multiple Random objects
        /// being seeded at the same time and producing identical sequences.
        /// </summary>
        private static readonly Random random = new();

        /// <summary>
        /// Pool of possible secret words.
        /// All lowercase since player input will be lowercased
        /// for comparison in Game.cs.
        /// </summary>
        private static readonly string[] words =
        [
            "programming",
            "hangman",
            "computer",
            "keyboard",
            "monitor",
            "software",
            "developer",
            "algorithm",
            "variable",
            "function",
            "compiler",
            "database",
            "network",
            "interface",
            "framework",
            "exception",
            "inheritance",
            "polymorphism",
            "encapsulation",
            "abstraction"
        ];

        /// <summary>
        /// Returns a randomly selected word from the words array.
        /// Uses the shared static Random instance to generate an index,
        /// avoiding repeated instantiation on every call.
        /// Called each time a new word is needed by Game.cs.
        /// </summary>
        /// <returns>A random lowercase word string</returns>
        public static string GetRandomWord()
        {
            int index = random.Next(0, words.Length);
            return words[index];
        }
    }
}