namespace TetrisClient {
    /// <summary>
    /// GameGrid, inherited from CustomGrid, is used to play the actual game on.
    /// </summary>
    public partial class GameGrid : CustomGrid {
        public GameGrid() : base(rows: Constants.ROWS, columns: Constants.COLUMNS, size: 30) {
            Draw();
        }
    }
}