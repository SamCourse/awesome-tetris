using System;
using System.Collections.Generic;

namespace TetrisEngine {
    internal enum Heading {
        LEFT,
        DOWN,
        RIGHT
    }
    
    public class TetrisGame {
        private int _rows;
        private int _columns;
        private List<Matrix> _queue { get; }
        private Shape _currentShape;
        private Board _board;
        
        public TetrisGame(int rows, int columns) {
            _rows = rows;
            _columns = columns;
            _queue = new List<Matrix>();
            _board = new Board(rows, columns);
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
        
        /// <returns> The current board as int[,] object. Can be used to update the view. </returns>
        public int[,] CurrentBoard() {
            return _board._board;
        }

        /// <summary>
        /// Tries to make the move with the given shape.
        /// </summary>
        /// <param name="heading">The direction of where the matrix should move.</param>
        /// <returns> True if the move was successful, false if unsuccessful </returns>
        private bool MakeMove(Heading heading) {
            // If the cell at the given position isn't set, there is no matrix there. Meaning the given position is not correct.
            if (!_board.CellIsSet(_currentShape.xPos, _currentShape.yPos)) {
                throw new InvalidMatrixPositionException(_currentShape.xPos, _currentShape.yPos);
            }

            int[,] matrixValue = _currentShape.matrix.Value;

            // Define two queues of actions that may or may not be executed.
            // All the delete actions need to be performed first, then the update actions afterwards.
            // This is to prevent newly placed coordinates being overwritten by delete actions of older coordinates that are performed later.
            List<Action> deleteActionQueue = new List<Action>();
            List<Action> updateActionQueue = new List<Action>();

            for (int y = 0; y < matrixValue.GetLength(0); y++) {
                for (int x = 0; x < matrixValue.GetLength(1); x++) {
                    // Get the type of the current position (and the entire tetromino if this is not 0)
                    int tetrominoType = matrixValue[x, y];
                    
                    // If at the current coordinates, there is a 0, skip this iteration.
                    if (tetrominoType == 0) {
                        continue;
                    }
                    
                    // Get the current coordinates of the single point in the matrix
                    int coordCurrX = _currentShape.xPos + x;
                    int coordCurrY = _currentShape.yPos + y;

                    int coordNewX = coordCurrX;
                    int coordNewY = coordCurrY;

                    // Update the coordinates based on the desired direction
                    switch (heading) {
                        case Heading.LEFT:
                            coordNewX--;
                            break;
                        case Heading.DOWN:
                            coordNewY++;
                            break;
                        case Heading.RIGHT:
                            coordNewX++;
                            break;
                    }

                    // If the cell at one of the new positions is already set, this action is not possible
                    if (_board.CellIsSet(coordNewX, coordNewY)) {
                        return false;
                    }

                    // Add the two actions to their respective queues, to be performed if none of the given cells are set.
                    deleteActionQueue.Add(() => _board.EmptyCell(coordCurrX, coordCurrY));
                    
                    updateActionQueue.Add(() => {
                        _currentShape.xPos = coordNewX;
                        _currentShape.yPos = coordNewY;
                        _board.SetCell(coordNewX, coordNewY, tetrominoType);
                    });
                }
            }
            
            foreach (Action deleteCell in deleteActionQueue) {
                deleteCell();
            }

            foreach (Action updateCellAndShape in updateActionQueue) {
                updateCellAndShape();
            }

            return true;
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

    /// <summary>
    /// Exception thrown when a Y and X coordinate is passed which does not contain a matrix.
    /// </summary>
    internal class InvalidMatrixPositionException : Exception {
        public InvalidMatrixPositionException(int posX, int posY) : 
            base($"No matrix found at coordinates [{posX}, {posY}].") {
        }
    }
}