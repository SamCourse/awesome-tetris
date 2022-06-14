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
        /// Different shapes have corresponding numbers. These can be found in Shapes.cs.
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
        /// <returns> Returns whether the given coordinates has a cell that is not equal to 0.</returns>
        internal bool CellIsSet(int x, int y) {
            return _board[y, x] == 0;
        }
    }
}