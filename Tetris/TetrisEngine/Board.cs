using System.Collections.Generic;
using System.Diagnostics.Contracts;
using static System.Linq.Enumerable;

namespace TetrisEngine {
    internal class Board {
        /// <summary>
        /// The board is represented as a list of a list of integers. Example:
        /// A board that looks like this: 0 0 1 1
        ///                               2 2 0 1
        ///                               2 0 0 1
        /// Would look like: [[0, 0, 1, 1], [2, 2, 0, 1], [2, 0, 0, 1]]
        /// </summary>
        internal int[,] _board { get; }

        /// <summary>
        /// Create a new board with the given row and column counts.
        /// </summary>
        /// <param name="rows">The amount of rows in the board.</param>
        /// <param name="columns">The amount of columns in the board.</param>
        internal Board(int rows, int columns) {
            _board = new int[rows, columns];
        }

        /// <summary>
        /// Set the cell at given coordinates equal to the type of shape.
        /// </summary>
        /// <param name="x"> The X-coordinate of the cell that needs to be set </param>
        /// <param name="y"> The Y-coordinate of the cell that needs to be set </param>
        /// <param name="type">
        /// The type of the shape that is placed here.
        /// Different shapes have corresponding numbers. See <see cref="Shapes.shapes"/>.
        /// 0 can be passed to clear the cell.
        /// </param>
        internal void SetCell(int x, int y, int type) {
            _board[y, x] = type;
        }

        /// <summary>
        /// Sets the cell at the given coordinates to 0.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell that needs to be emptied</param>
        /// <param name="y">The Y-coordinate of the cell that needs to be emptied</param>
        internal void EmptyCell(int x, int y) {
            SetCell(x, y, 0);
        }

        /// <summary>
        /// A check to validate whether a coordinate on the board is already set.
        /// </summary>
        /// <param name="x"> The X-coordinate of the cell that needs to be checked</param>
        /// <param name="y"> The Y-coordinate of the cell that needs to be checked</param>
        /// <returns> Returns whether the value of the cell at the given coordinates is not equal to 0.</returns>
        [Pure]
        private bool CellIsSet(int x, int y) {
            return _board[y, x] is not 0 and not -1;
        }

        private bool IsOutOfBounds(int x, int y) {
            int boardHeight = _board.GetLength(0);
            int boardWidth = _board.GetLength(1);

            return x < 0 || x >= boardWidth ||
                   y < 0 || y >= boardHeight;
        }

        /// <summary>
        /// Checks the board for rows that are full.
        /// </summary>
        /// <returns>A IEnumerator of row indexes that are full.</returns>
        [Pure]
        internal IEnumerator<int> GetCompleteRows() {
            return
                Range(0, _board.GetLength(0))
                    .Where(y =>
                        Range(0, _board.GetLength(1))
                            .All(x => CellIsSet(x, y))
                    ).GetEnumerator();
        }

        /// <summary>
        /// Drops all the floating rows down by one if a row was deleted.
        /// </summary>
        /// <param name="deletedRow">The row that was deleted</param>
        internal void DropFloatingRows(int deletedRow) {
            for (int y = deletedRow; y > 0; y--) // Start at deleted row, move all the way up
            for (int x = 0; x < _board.GetLength(1); x++) // Iterate over the columns on this row
                _board[y, x] = _board[y - 1, x]; // Copy the row above
        }


        internal bool CanSpawnNew(Tetromino tetromino) {
            tetromino.xPos = _board.GetLength(1) / 2 - 1;

            return !tetromino.Coordinates.Any(coordinate => CellIsSet(coordinate.Item1, coordinate.Item2));
        }

        internal void SpawnNew(Tetromino tetromino) {
            foreach ((int x, int y) in tetromino.Coordinates)
                SetCell(x, y, tetromino.Type);
        }

        [Pure]
        internal bool CanPlace(Tetromino newTetromino, Tetromino oldTetromino) {
            return !newTetromino.Coordinates.Any(coordinate => {
                (int x, int y) = coordinate;
                return IsOutOfBounds(x, y) || !oldTetromino.IsOnCoordinates(x, y) && CellIsSet(x, y);
            });
        }

        internal void RemoveGhostPiece() {
            int matrixHeight = _board.GetLength(0);
            int matrixWidth = _board.GetLength(1);

            for (int y = 0; y < matrixHeight; y++)
            for (int x = 0; x < matrixWidth; x++)
                if (_board[y, x] == -1)
                    _board[y, x] = 0;
        }
    }
}