import { useEffect, useMemo, useState } from 'react'
import './App.css'
import { Board } from './sokoban/Board.jsx'
import { createInitialState, isLevelComplete, movePlayer } from './sokoban/gameLogic.js'
import { tileMap01 } from './sokoban/level.js'

function App() {
  /*
    React state is the app's memory.

    We store the whole Sokoban game state here because changing this value should
    update several parts of the screen at once:
    - the board cells
    - the player position
    - the block positions
    - the move/push counters
    - the win message

    The function form of useState runs only once when the component first loads,
    which is useful because creating the initial board requires scanning the map.
  */
  const [gameState, setGameState] = useState(() => createInitialState(tileMap01))

  /*
    The movement rules do not need to know how the player looks.

    This separate state only controls the sprite direction. It changes after a
    valid move so the character faces the direction they actually moved.
  */
  const [playerDirection, setPlayerDirection] = useState('down')

  /*
    Derived state means "a value we can calculate from existing state".

    We do not store `hasWon` separately with useState because that would give us
    two sources of truth. If the blocks move, React re-renders, and this value is
    recalculated from the current block positions.
  */
  const hasWon = useMemo(() => isLevelComplete(gameState), [gameState])

  function resetLevel() {
    setGameState(createInitialState(tileMap01))
    setPlayerDirection('down')
  }

  function handleMove(direction) {
    /*
      The movement rules live outside React in gameLogic.js.

      That keeps App.jsx focused on app behavior:
      - receive input
      - update state
      - render UI

      The previous state is passed into movePlayer, and movePlayer returns either
      the unchanged state or a new state with updated player/block positions.
    */
    setGameState((currentState) => {
      const nextState = movePlayer(currentState, direction)

      /*
        movePlayer returns the same object when movement is blocked. Checking
        identity here prevents the sprite from turning toward a wall or blocked
        crate when the player did not actually move.
      */
      if (nextState !== currentState) {
        setPlayerDirection(direction)
      }

      return nextState
    })
  }

  useEffect(() => {
    function handleKeyDown(event) {
      /*
        Arrow keys normally scroll the page. preventDefault stops that browser
        behavior so the keys feel like game controls.
      */
      if (event.key.startsWith('Arrow')) {
        event.preventDefault()
      }

      /*
        The UI uses readable direction words. gameLogic.js translates those words
        into row/column changes.
      */
      if (event.key === 'ArrowUp') handleMove('up')
      if (event.key === 'ArrowDown') handleMove('down')
      if (event.key === 'ArrowLeft') handleMove('left')
      if (event.key === 'ArrowRight') handleMove('right')
    }

    window.addEventListener('keydown', handleKeyDown)

    /*
      Effects can clean up after themselves.

      If this component is ever removed from the page, React will run this return
      function and remove the keyboard listener. That prevents duplicate keyboard
      handlers if the component is mounted again later.
    */
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [])

  return (
    <main className="app-shell">
      <header className="top-bar">
        <div>
          <h1>Sokoban</h1>
          <p className="subtitle">Push every block onto a goal tile.</p>
          <p className="controls-text">Use the arrow keys to move.</p>
        </div>

        <button className="reset-button" type="button" onClick={resetLevel}>
          Reset
        </button>
      </header>

      <section className="status-row" aria-label="Game status">
        <div>
          <span className="status-label">Moves</span>
          <strong>{gameState.moves}</strong>
        </div>
        <div>
          <span className="status-label">Pushes</span>
          <strong>{gameState.pushes}</strong>
        </div>
        <div>
          <span className="status-label">Blocks on goals</span>
          <strong>
            {gameState.blocksOnGoals} / {gameState.blocks.length}
          </strong>
        </div>
      </section>

      {hasWon && (
        <p className="win-message" role="status">
          Level complete. Press Reset to play it again.
        </p>
      )}

      <Board gameState={gameState} playerDirection={playerDirection} />
    </main>
  )
}

export default App
