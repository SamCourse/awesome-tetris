using System;
using System.Collections.Generic;

namespace TetrisEngine {
    
    public class TetrisGame {
        private int _rows;
        private int _columns;
        private List<Matrix> _queue { get; }
        private Shape _currentShape;
        
        public TetrisGame(int rows, int columns) {
            _rows = rows;
            _columns = columns;
            _queue = new List<Matrix>();
        }

        /// <summary>
        /// Initializes the game. TODO: More info
        /// </summary>
        public void InitializeGame() {
            // Queue 3 shapes
            for (int i = 0; i < 3; i++) {
                QueueNewShape();
            }
        }

        /// <summary>
        /// Moves the next tetromino from the queue to the board and adds a new random tetromino to the queue.
        /// </summary>
        private void SpawnNextTetromino() {
            // Get and removes the next tetromino from the queue
            Matrix matrix = _queue[0];
            _queue.RemoveAt(0);
            
            // Add a new tetromino to the queue
            QueueNewShape();

            // Create a new shape with the randomly picked tetromino and the default starting position
            _currentShape = new Shape(matrix, _columns / 2 - 1, 0);
        }

        /// <summary>
        /// Adds a new random tetromino to the queue.
        /// </summary>
        private void QueueNewShape() {
            _queue.Add(Shapes.RandomShape());
        }
    }

}