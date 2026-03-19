# Hangman

## Introduction
Hangman is a classic pen and paper guessing game. The application picks a secret word
and the player tries to guess it one letter at a time, or by guessing the whole word
at once. Each wrong guess brings the hangman one step closer to his fate, guess the
word before he runs out of chances!

---

## Assignment Requirements

### Game Rules
- The player has **10 guesses** to complete the word before losing.
- Two types of guesses are supported:
  - **Letter guess** — if the letter exists in the word, all matching positions are revealed.
  - **Full word guess** — if correct, the player wins immediately. If wrong, nothing is revealed.
- Guessing the same letter twice does **not** consume a guess.

### Code Requirements
- The secret word is randomly chosen from an array of strings.
- Incorrect letter guesses are tracked and displayed to the player after each guess.
- Correct letters are stored in a `char[]`. Unrevealed letters are represented by `_`.

### Extras
- `.gitignore` for the project.
- `README.md` for the project.

---

## What We Built
- **Main menu** with options to start a new game or quit.
- **Pause menu** accessible during a game, with options to resume, start a new game, or quit.
- **Letter guessing** that reveals all matching positions in the word.
- **Full word guessing** that wins the game instantly if correct.
- **Duplicate guess detection** for both letters and full words — duplicates are flagged without consuming a guess.
- **Progressive ASCII art** with 11 stages that build up the hangman as wrong guesses accumulate.
- **Live game state display** showing guesses remaining, incorrect letters guessed, words guessed, and the current revealed word.

---

## Why We Did It This Way

### Separation of concerns
The project is split across focused files rather than putting everything in one class:
- `Game.cs` — core game logic only
- `Menu.cs` — all menu screens and navigation
- `WordBank.cs` — word storage and random selection
- `Art.cs` — ASCII art stages

This makes each file easy to read, modify, and extend independently.

### `lastMessage` pattern
Instead of printing feedback directly inside guess handlers, feedback is stored in
`lastMessage` and displayed on the next redraw by `DisplayState()` as the Status line.
This decouples guess results from the display cycle — guess handlers don't need to know
anything about when or how the screen is drawn, they simply set a message and move on.

### Shared `Random` instance in `WordBank`
`Random` is declared as a `static readonly` field rather than being instantiated inside
`GetRandomWord()` on every call. This avoids the risk of multiple `Random` objects being
created close together in time, which can result in identical seeds and repeated words.

### `Run()` owns the restart loop
When the player chooses New Game from the pause menu, `GameLoop()` returns `true` to
signal a restart rather than calling `new Game().Run()` recursively. This keeps the call
stack flat no matter how many games are played in a single session.

### `Console.Clear()` and `\x1b[3J`
Every redraw calls both `Console.Clear()` and `Console.WriteLine("\x1b[3J")` together.
`Console.Clear()` clears the visible terminal window but on some systems leaves the
scrollback buffer intact, meaning the player could scroll up and see previous game states.
`\x1b[3J` is an ANSI escape code that clears the scrollback buffer as well, ensuring
the terminal is completely clean on every redraw with no history leaking through.