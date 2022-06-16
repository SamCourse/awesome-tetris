using System.Collections.Generic;
using System.Linq;
using TetrisEngine;

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
        
        /// <summary>
        /// Updates the grid with any given changes.
        /// </summary>
        public override void UpdateBoard(TetrisGame game) {
            // Clear the grid
            ClearGrid();

            // Get the list of matrices that are in the queue from the engine
            List<Matrix> queue = game.Queue.ToList();

            for (int i = 0; i < queue.Count; i++) {
                int shapeYStartPosition = i * 3; // Every shape will be positioned 3 cells lower than the previous.
                int[,] matrix = queue[i].Value;

                // Draw the queue cells in the queue grid
                for (int y = 0; y < matrix.GetLength(0); y++)
                for (int x = 0; x < matrix.GetLength(1); x++)
                    DrawCell(x, y + shapeYStartPosition, matrix[y, x]);
            }
        }
    }
}