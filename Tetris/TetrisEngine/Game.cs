using System;
using System.Collections.Generic;

namespace TetrisEngine {
    
    public class TetrisGame {
        private int _rows;
        private int _columns;
        private List<Matrix> _queue { get; }
        
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
        /// Adds a new random tetromino to the queue.
        /// </summary>
        private void QueueNewShape() {
            _queue.Add(Shapes.RandomShape());
        }
    }

}