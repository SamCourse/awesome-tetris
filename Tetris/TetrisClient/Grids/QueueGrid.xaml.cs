namespace TetrisClient {
    /// <summary>
    /// QueueGrid, inherited from CustomGrid, is used to display the queue for the Tetris game.
    /// </summary>
    public partial class QueueGrid : CustomGrid {
        public QueueGrid() : base(rows: 8, columns: 4, size: 25) {
            ShowBorders = false;
            Draw();
        }
    }
}