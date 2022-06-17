namespace TetrisClient
{
    /// <summary>
    /// Interaction logic for QueueGrid.xaml
    /// </summary>
    public partial class QueueGrid : CustomGrid {
        public QueueGrid() : base(rows: 8, columns: 4, size: 25) {
            ShowBorders = false;
            Draw();
        }
    }
}