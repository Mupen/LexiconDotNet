export function Tile({ cell, playerDirection }) {
  /*
    One Sokoban cell can have a static floor type and a moving entity.

    Static tile:
    - wall
    - space
    - goal

    Moving entity:
    - player
    - block

    The CSS classes below let styling handle the visuals while this component
    stays focused on deciding what should be present in the cell.
  */
  const classNames = ['cell', `cell-${cell.tile}`]

  if (cell.containsPlayer) {
    classNames.push('cell-player')
  }

  if (cell.containsBlock) {
    classNames.push(cell.blockIsOnGoal ? 'cell-block-done' : 'cell-block')
  }

  return (
    <div className={classNames.join(' ')}>
      {cell.tile === 'goal' && <span className="goal-dot" aria-hidden="true" />}
      {cell.containsBlock && <span className="block" aria-hidden="true" />}
      {cell.containsPlayer && (
        <span className={`player player-${playerDirection}`} aria-hidden="true" />
      )}
    </div>
  )
}
