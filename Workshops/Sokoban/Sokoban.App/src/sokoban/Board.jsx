import { getCellView } from './gameLogic.js'
import { Tile } from './Tile.jsx'

export function Board({ gameState, playerDirection }) {
  /*
    CSS grid needs to know how many columns the level has.

    The assignment level is 19 columns wide and 16 rows tall. We pass those
    numbers through CSS custom properties so the CSS can build the grid without
    hard-coding this exact level size.
  */
  const boardStyle = {
    '--board-columns': gameState.width,
    '--board-rows': gameState.height,
  }

  return (
    <section className="board-wrap" aria-label="Sokoban board">
      <div className="board" style={boardStyle}>
        {gameState.tiles.map((row, rowIndex) =>
          row.map((_, colIndex) => {
            /*
              React needs a stable key when rendering lists. A row/column key is
              stable because each cell always represents the same board position.
            */
            const key = `${rowIndex}-${colIndex}`
            const cell = getCellView(gameState, rowIndex, colIndex)

            return <Tile key={key} cell={cell} playerDirection={playerDirection} />
          }),
        )}
      </div>
    </section>
  )
}
