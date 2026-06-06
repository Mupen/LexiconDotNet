# Sokoban

## Introduction
Sokoban is a grid-based puzzle game where the player pushes blocks onto goal tiles.

This workshop version is built as a React and Vite frontend application. The project uses the original assignment map data, converts it into cleaner game state, and renders the board with React components.

The game runs fully in the browser. There is no backend API for this version.

---

## Assignment Scope

The assignment focuses on implementing the Sokoban rules:

- render a fixed board from the provided map data
- show walls, empty spaces, goals, blocks, and the player
- move the player with keyboard input
- prevent movement through walls
- allow pushing blocks
- prevent pushing blocks into walls or other blocks
- detect when all blocks are on goal tiles

Manual requirement checklist:

- player can move with the arrow keys
- player cannot move outside the board
- player cannot walk through walls
- player can push one block at a time
- player cannot push two blocks at once
- player cannot push a block into a wall
- move counter increases after valid movement
- push counter increases after valid block pushes
- blocks-on-goals counter updates when blocks reach goal tiles
- win message appears when all blocks are on goals

This version also adds:

- React component structure
- separated game logic
- move and push counters
- reset button
- directional player sprites
- responsive board sizing
- Docker support with Nginx

---

## What I Built

The app includes:

- a playable Sokoban level based on the workshop starter map
- keyboard controls using the arrow keys
- move counter
- push counter
- blocks-on-goals counter
- win message when the level is complete
- reset button
- directional player sprite that changes when the player moves
- larger board cells for better readability
- production Docker image served with Nginx

The visual style is intentionally simple. The board uses colored tiles for walls, floor, goals, and blocks, while the player uses sprites from the example project.

---

## Asset Credits

The player sprite assets used in this project come from Boris Paskhaver's Sokoban project:

```text
https://github.com/paskhaver/sokoban/tree/master
```

Only the directional player sprites are currently used. Other visual assets from that example were reviewed but not kept in the final version because the simple colored board was easier to read for this workshop implementation.

---

## Project Structure

```text
Sokoban.App
  Dockerfile
  nginx.conf
  package.json
  index.html
  public/
    sprites/
      player/
        player-down.png
        player-side.png
        player-up.png
  src/
    main.jsx
    index.css
    App.jsx
    App.css
    sokoban/
      level.js
      gameLogic.js
      Board.jsx
      Tile.jsx
```

Important files:

```text
src/sokoban/level.js       original assignment map data, exported for React
src/sokoban/gameLogic.js   Sokoban rules and state conversion
src/sokoban/Board.jsx      renders the full grid
src/sokoban/Tile.jsx       renders one board cell
src/App.jsx                owns game state, keyboard input, reset, and win state
src/App.css                board layout, tile styling, and player sprites
```

---

## Why I Did It This Way

### React owns the UI state

The current board, player position, block positions, move count, and push count live in React state. When the player moves, React receives a new state and re-renders the board.

### Game logic is separate from rendering

The movement rules live in `gameLogic.js`, not inside the React components. This keeps the rules easier to understand because they do not depend on JSX or browser rendering.

### The original map is not mutated

The assignment map mixes static tiles and moving entities in one grid. The app converts it into a cleaner shape:

```text
tiles   static walls, spaces, and goals
player  current player position
blocks  current block positions
```

This makes movement simpler because walls and goals never move, while blocks and the player do.

### Player sprites are visual only

The player direction is tracked in `App.jsx` and passed down to the tile renderer. The game rules do not need to know which sprite is displayed.

### Tile sprites are left as a future option

The app briefly tested wall, crate, floor, and goal sprites from the example project, but the result was less readable. The CSS now keeps a comment showing where tile sprites can be tested later without changing React or the game logic.

---

## Requirements

- Node.js
- npm
- a modern browser
- Docker Desktop, only if running the Docker version

---

## Running the App

Detailed run, build, and Docker commands are kept in the app-level README:

```text
Sokoban.App/README.md
```

This keeps the workshop README focused on the assignment, design decisions, and project structure, while the app README works as the command reference.

---

## Current Status

Completed:

- board rendering
- player movement
- block pushing
- wall and block collision rules
- move and push counters
- win detection
- reset button
- directional player sprites
- Dockerfile
- Nginx static hosting
- README

Not currently included:

- multiple levels
- undo
- best score storage
- automated tests
- solver/minimum move calculation
- animated movement

---

## AI Disclosure

This project was written by Daniel Henriksen. ChatGPT (AI by OpenAI) was used as a collaborative tool throughout the process.

The project direction, gameplay choices, and final design decisions are Daniel Henriksen's.
