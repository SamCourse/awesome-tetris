namespace TetrisClient {
    /// <summary>
    /// GameGrid, used to play Tetris on
    /// </summary>
    public partial class GameGrid : CustomGrid {
        public GameGrid() : base(rows: Constants.ROWS, columns: Constants.COLUMNS, size: 30) {
            Draw();
        }
    }
}