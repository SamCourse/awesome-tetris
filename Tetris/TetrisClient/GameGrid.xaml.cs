namespace TetrisClient
{
    /// <summary>
    /// GameGrid, used to play Tetris on
    /// </summary>
    public partial class GameGrid : CustomGrid
    {
        public GameGrid() : base(rows: 16, columns: 10, size: 30)
        {
            Draw();
        }
    }
}