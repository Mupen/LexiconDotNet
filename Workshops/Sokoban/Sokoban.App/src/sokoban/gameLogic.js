/*
  This file contains the rules of Sokoban.

  It does not import React and it does not render anything. That is deliberate:
  game rules are easier to understand and test when they are just JavaScript
  functions that receive data and return data.
*/

const TILE = {
  SPACE: 'space',
  WALL: 'wall',
  GOAL: 'goal',
}

const DIRECTIONS = {
  up: { row: -1, col: 0 },
  down: { row: 1, col: 0 },
  left: { row: 0, col: -1 },
  right: { row: 0, col: 1 },
}

export function createInitialState(tileMap) {
  const tiles = []
  const blocks = []
  let player = null

  /*
    The assignment map stores everything in one grid:
    - W is a wall
    - G is a goal
    - B is a block
    - P is the player

    For gameplay, walls/goals are static, while blocks/player move. This loop
    splits those concepts into separate pieces of state.
  */

  tileMap.mapGrid.forEach((row, rowIndex) => {
    const tileRow = []

    row.forEach((cell, colIndex) => {
      /*
        Each assignment cell is an array like ['W'], so cell[0] gives the symbol.
      */
      const symbol = cell[0]

      if (symbol === 'W') {
        tileRow.push(TILE.WALL)
        return
      }

      if (symbol === 'G') {
        tileRow.push(TILE.GOAL)
        return
      }

      if (symbol === 'B') {
        /*
          A block starts on a normal floor tile. The block itself goes into the
          moving blocks array.
        */
        tileRow.push(TILE.SPACE)
        blocks.push({ row: rowIndex, col: colIndex })
        return
      }

      if (symbol === 'P') {
        /*
          The player also starts on a normal floor tile. We keep only one player
          position because Sokoban has one player.
        */
        tileRow.push(TILE.SPACE)
        player = { row: rowIndex, col: colIndex }
        return
      }

      tileRow.push(TILE.SPACE)
    })

    tiles.push(tileRow)
  })

  if (!player) {
    throw new Error('The Sokoban map must contain one player start position.')
  }

  const state = {
    width: tileMap.width,
    height: tileMap.height,
    tiles,
    player,
    blocks,
    moves: 0,
    pushes: 0,
  }

  return addDerivedCounts(state)
}

export function movePlayer(state, directionName) {
  const direction = DIRECTIONS[directionName]

  if (!direction) {
    return state
  }

  const nextPlayerPosition = {
    row: state.player.row + direction.row,
    col: state.player.col + direction.col,
  }

  /*
    The player cannot move outside the map or into a wall.
  */
  if (!isInsideBoard(state, nextPlayerPosition) || isWall(state, nextPlayerPosition)) {
    return state
  }

  const blockIndex = findBlockIndex(state.blocks, nextPlayerPosition)

  /*
    If the next cell has no block, this is a normal step. We create a new state
    object instead of mutating the old one because React state should be treated
    as immutable.
  */
  if (blockIndex === -1) {
    return addDerivedCounts({
      ...state,
      player: nextPlayerPosition,
      moves: state.moves + 1,
    })
  }

  /*
    If there is a block in the next cell, the player is trying to push it.
    Sokoban only allows pushing, not pulling, so we inspect the cell after the
    block in the same direction.
  */
  const nextBlockPosition = {
    row: nextPlayerPosition.row + direction.row,
    col: nextPlayerPosition.col + direction.col,
  }

  const blockIsBlocked =
    !isInsideBoard(state, nextBlockPosition) ||
    isWall(state, nextBlockPosition) ||
    hasBlock(state.blocks, nextBlockPosition)

  if (blockIsBlocked) {
    return state
  }

  /*
    Create a new blocks array where only the pushed block has changed position.
    All other blocks keep their old coordinates.
  */
  const movedBlocks = state.blocks.map((block, index) =>
    index === blockIndex ? nextBlockPosition : block,
  )

  return addDerivedCounts({
    ...state,
    player: nextPlayerPosition,
    blocks: movedBlocks,
    moves: state.moves + 1,
    pushes: state.pushes + 1,
  })
}

export function isLevelComplete(state) {
  /*
    A level is complete when every block stands on a goal tile.
  */
  return state.blocks.every((block) => tileAt(state, block) === TILE.GOAL)
}

export function getCellView(state, row, col) {
  /*
    React's Board component needs one combined answer per cell:
    - what static tile is here?
    - is the player here?
    - is a block here?
    - is this block already on a goal?

    This function keeps that lookup logic out of the JSX.
  */
  const position = { row, col }
  const tile = tileAt(state, position)
  const containsPlayer = positionsMatch(state.player, position)
  const containsBlock = hasBlock(state.blocks, position)

  return {
    tile,
    containsPlayer,
    containsBlock,
    blockIsOnGoal: containsBlock && tile === TILE.GOAL,
  }
}

function addDerivedCounts(state) {
  /*
    blocksOnGoals can always be calculated from blocks + tiles. We attach it to
    the state because it is convenient for the status UI, but we still calculate
    it in one place so the number stays consistent.
  */
  return {
    ...state,
    blocksOnGoals: state.blocks.filter((block) => tileAt(state, block) === TILE.GOAL).length,
  }
}

function isInsideBoard(state, position) {
  return (
    position.row >= 0 &&
    position.row < state.height &&
    position.col >= 0 &&
    position.col < state.width
  )
}

function tileAt(state, position) {
  return state.tiles[position.row][position.col]
}

function isWall(state, position) {
  return tileAt(state, position) === TILE.WALL
}

function hasBlock(blocks, position) {
  return findBlockIndex(blocks, position) !== -1
}

function findBlockIndex(blocks, position) {
  return blocks.findIndex((block) => positionsMatch(block, position))
}

function positionsMatch(first, second) {
  return first.row === second.row && first.col === second.col
}
