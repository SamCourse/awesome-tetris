using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetrisEngine {
    internal enum Heading {
        LEFT,
        DOWN,
        RIGHT,
        NONE
    }
    
    public class TetrisGame {
        private int _rows;
        private int _columns;
        private List<Matrix> _queue { get; }
        private Tetromino _currentTetromino;
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
            // Queue 3 tetromino's
            for (int i = 0; i < 3; i++) {
                QueueNewTetromino();
            }

            SpawnNextTetromino();
            FallTimer();
        }

        private async void FallTimer() {
            while (true) {
                await Task.Delay(1000);
                MoveDown();
            }
        }

        /// <returns> The current board as int[,] object. Can be used to update the view. </returns>
        public int[,] CurrentBoard() {
            return _board._board;
        }

        /// <summary>
        /// Tries to make the move with the current tetromino in the given direction.
        /// </summary>
        /// <param name="heading">The direction of where the tetromino should move.</param>
        /// <returns> True if the move was successful, false if unsuccessful </returns>
        private bool AttemptMove(Heading heading) {
            int[,] matrixValue = _currentTetromino.matrix.Value;

            // Define two queues of actions that may or may not be executed.
            // All the delete actions need to be performed first, then the update actions afterwards.
            // This is to prevent newly placed coordinates being overwritten by delete actions of older coordinates that are performed later.
            List<Action> deleteActionQueue = new List<Action>();
            List<Action> updateActionQueue = new List<Action>();

            int matrixHeight = matrixValue.GetLength(0);
            int matrixWidth = matrixValue.GetLength(1);

            for (int y = 0; y < matrixHeight; y++) {
                for (int x = 0; x < matrixWidth; x++) {
                    // Get the type of the current position (and the entire tetromino if this is not 0)
                    int tetrominoType = matrixValue[y, x];

                    // If at the current coordinates, there is a 0, skip this iteration.
                    if (tetrominoType == 0)
                        continue;

                    // Get the current coordinates of the single point in the matrix
                    int coordCurrX = _currentTetromino.xPos + x;
                    int coordCurrY = _currentTetromino.yPos - (matrixHeight - 1) + y;

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

                    if (coordNewX > _columns - 1 || coordNewX < 0) { // If out of bounds on the side, do nothing
                        return true;
                    }

                    if (coordNewY > _rows - 1) { // If at bottom, finish this piece
                        return false;
                    }

                    // If the cell at one of the new positions is already set and
                    // it is not the same tetromino's old cells, this action is not possible
                    if (_board.CellIsSet(coordNewX, coordNewY) &&
                        !_currentTetromino.IsOnCoordinates(coordNewX, coordNewY)) {
                        return heading != Heading.DOWN; // If the move is not DOWN but LEFT or RIGHT,
                                                        // don't process move but keep playing with same piece
                    }

                    // Add the two actions to their respective queues, to be performed if none of the given cells are set.
                    deleteActionQueue.Add(() => _board.EmptyCell(coordCurrX, coordCurrY));
                    updateActionQueue.Add(() => _board.SetCell(coordNewX, coordNewY, tetrominoType));
                }
            }


            switch (heading) {
                case Heading.DOWN:
                    _currentTetromino.yPos++;
                    break;
                case Heading.LEFT:
                    _currentTetromino.xPos--;
                    break;
                case Heading.RIGHT:
                    _currentTetromino.xPos++;
                    break;
            }

            foreach (Action deleteCell in deleteActionQueue)
                deleteCell();

            foreach (Action updateCellAndTetromino in updateActionQueue)
                updateCellAndTetromino();

            return true;
        }

        /// <summary>
        /// Tries to move the tetromino. If unsuccesful, spawns the next one in the queue.
        /// </summary>
        /// <param name="heading">The direction the move is in.</param>
        private void Move(Heading heading) {
            bool moveSuccesful = AttemptMove(heading);

            if (!moveSuccesful) {
                SpawnNextTetromino();
        }

        /// <summary>
        /// Moves the current tetromino to the left
        /// </summary>
        public void MoveLeft() {
            Move(Heading.LEFT);
        }

        /// <summary>
        /// Moves the current tetromino to the right
        /// </summary>
        public void MoveRight() {
            Move(Heading.RIGHT);
        }

        /// <summary>
        /// Moves the current tetromino down
        /// </summary>
        public void MoveDown() {
            Move(Heading.DOWN);
        }

        public void Rotate() {
            Tetromino tetrominoPreRotate = _currentTetromino;
            Matrix rotatedMatrix = _currentTetromino.matrix.Rotate90();

            Tetromino postRotate = new Tetromino(rotatedMatrix, _currentTetromino.xPos, _currentTetromino.yPos);

            // Like the Move() method, the actions in these queues are only executed after all the checks pass.
            List<Action> deleteActionQueue = new List<Action>();
            List<Action> updateActionQueue = new List<Action>();

            for (int y = 0; y < rotatedMatrix.Value.GetLength(0); y++) {
                for (int x = 0; x < rotatedMatrix.Value.GetLength(1); x++) {
                    // Get the tetromino type of the current position
                    int tetrominoType = rotatedMatrix.Value[y, x];

                    // Get the coordinates of the current single point in the matrix
                    int rotatedCoordX = postRotate.xPos + x;
                    int rotatedCoordY = postRotate.yPos - (rotatedMatrix.Value.GetLength(0) - 1) + y;

                    if (rotatedCoordX > _columns - 1 || rotatedCoordX < 0) { // If out of bounds on the side, do nothing
                        return;
                    }

                    // If the cell at one of the new positions is already set and
                    // it is not the cells of the pre-rotation matrix, this action is not possible
                    if (rotatedCoordY > _rows - 1 || // If at bottom, rotation not possible
                        _board.CellIsSet(rotatedCoordX, rotatedCoordY) &&
                        !tetrominoPreRotate.IsOnCoordinates(rotatedCoordX, rotatedCoordY)) {
                        return;
                    }

                    // Add the two actions to their respective queues, to be performed if none of the given cells are set.
                    deleteActionQueue.Add(() => _board.EmptyCell(rotatedCoordX, rotatedCoordY));
                    updateActionQueue.Add(() => _board.SetCell(rotatedCoordX, rotatedCoordY, tetrominoType));
                }
            }

            _currentTetromino = postRotate;

            foreach (Action deleteCell in deleteActionQueue)
                deleteCell();

            foreach (Action updateCellAndTetromino in updateActionQueue)
                updateCellAndTetromino();
        }

        /// <summary>
        /// Moves the next tetromino from the queue to the board and adds a new random tetromino to the queue.
        /// </summary>
        private void SpawnNextTetromino() {
            // Get and removes the next tetromino from the queue
            Matrix matrix = _queue[0];
            _queue.RemoveAt(0);

            // Add a new tetromino to the queue
            QueueNewTetromino();

            // Create a new tetromino with the randomly picked shape and the default starting position
            _currentTetromino = new Tetromino(
                matrix,
                _columns / 2 - 1,
                matrix.Value.GetLength(0) - 1);

            Move(Heading.NONE);
        }
        }

        /// <summary>
        /// Adds a new random tetromino to the queue.
        /// </summary>
        private void QueueNewTetromino() {
            _queue.Add(Shapes.RandomShape());
        }
    }
}