using System;
using System.Collections.Generic;
using System.Timers;

namespace TetrisEngine {
    internal enum Direction {
        LEFT,
        DOWN,
        RIGHT
    }
    
    internal enum GameState {
        CREATED,
        PLAYING,
        PAUSED, // TODO
        OVER
    }

    public class TetrisGame {
        private int _rows;
        private int _columns;
        private readonly Queue<Matrix> _queue;
        private Tetromino _currentTetromino;
        private Board _board;
        private Timer _timer;
        private Scoring _scoring;
        private GameState _gameState;
        private Random _random;
        
        public int[,] Board => _board._board;
        public int Points => _scoring.Points;
        public int Lines => _scoring.Lines;
        public int[,] Queue => Utils.QueueToIntArr(_queue);
        

        public TetrisGame(int rows, int columns, int seed) {
            _rows = rows;
            _columns = columns;
            _queue = new Queue<Matrix>();
            _board = new Board(rows, columns);
            _gameState = GameState.CREATED;
            _random = seed == 0 ? new Random() : new Random(seed);
        }

        /// <summary>
        /// Initializes the game. Sets the gamestate and fills the tetromino queue.
        /// Spawns the first tetromino, and sets up the automatic falling timer.
        /// </summary>
        public void InitializeGame() {
            // Initialize the scoring system with the Normal-mode scoring system.
            _scoring = Scoring.NormalGame();
            
            // Set the gamestate to playing.
            _gameState = GameState.PLAYING;

            // Queue 3 tetromino's
            for (int i = 0; i < 3; i++) {
                QueueNewTetromino();
            }

            AttemptSpawnNextTetromino();
            SetupFallTimer();
        }

        /// <summary>
        /// Sets up the timer that handles the tetromino being dropped every x milliseconds.
        /// </summary>
        private void SetupFallTimer() {
            _timer = new Timer(1000);

            // Register the event for when a piece falls
            _timer.Elapsed += (_, _) => {
                Move(Direction.DOWN);
                _scoring.Fall();
            };

            _timer.Start();
        }

        /// <summary>
        /// Tries to make the move with the current tetromino in the given direction.
        /// </summary>
        /// <param name="direction">The direction of where the tetromino should move.</param>
        /// <returns> True if the move was successful, false if unsuccessful</returns>
        private bool AttemptMove(Direction direction) {
            int[,] matrixValue = _currentTetromino.matrix.Value;

            // Define two queues of actions that may or may not be executed.
            // All the delete actions need to be performed first, then the update actions afterwards.
            // This is to prevent newly placed coordinates being overwritten by delete actions of older coordinates that are performed later.
            List<Action> deleteActionQueue = new List<Action>();
            List<Action> updateActionQueue = new List<Action>();

            int matrixHeight = matrixValue.GetLength(0);
            int matrixWidth = matrixValue.GetLength(1);

            for (int y = 0; y < matrixHeight; y++)
            for (int x = 0; x < matrixWidth; x++) {
                // Get the type of the current position (and the entire tetromino if this is not 0)
                int tetrominoType = matrixValue[y, x];

                // If there is a 0 at the current coordinates, skip this iteration.
                if (tetrominoType == 0)
                    continue;

                // Get the current coordinates of the single point in the matrix
                int coordCurrX = _currentTetromino.xPos + x;
                int coordCurrY = _currentTetromino.yPos - (matrixHeight - 1) + y;

                // Define copies of current coordinates to be used for new coordinates.
                int coordNewX = coordCurrX;
                int coordNewY = coordCurrY;

                // Update the coordinates based on the desired direction
                switch (direction) {
                    case Direction.LEFT:
                        coordNewX--;
                        break;
                    case Direction.DOWN:
                        coordNewY++;
                        break;
                    case Direction.RIGHT:
                        coordNewX++;
                        break;
                }

                if (coordNewX >= _columns || coordNewX < 0) { // If out of bounds on the side, do nothing
                    return true;
                }

                if (coordNewY >= _rows) { // If at bottom, return false to finish this piece
                    return false;
                }

                // If the cell at one of the new positions is already set and
                // it is not the same tetromino's old cells, this action is not possible
                if (_board.CellIsSet(coordNewX, coordNewY) &&
                    !_currentTetromino.IsOnCoordinates(coordNewX, coordNewY)) {
                    return direction != Direction.DOWN; // If the move is not DOWN but LEFT or RIGHT,
                    // don't process move but keep playing with same piece
                }

                // Add the two actions to their respective queues, to be performed if none of the given cells are set.
                deleteActionQueue.Add(() => _board.EmptyCell(coordCurrX, coordCurrY));
                updateActionQueue.Add(() => _board.SetCell(coordNewX, coordNewY, tetrominoType));
            }


            switch (direction) {
                case Direction.DOWN:
                    _currentTetromino.yPos++;
                    break;
                case Direction.LEFT:
                    _currentTetromino.xPos--;
                    break;
                case Direction.RIGHT:
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
        /// Tries to move the current tetromino.
        /// If unsuccesful, updates the score and checks for any completed rows. Then tries to spawn the next tetromino.
        /// If the spawn was unsuccesful, ends the game.
        /// </summary>
        /// <param name="direction">The direction the move is in.</param>
        private void Move(Direction direction) {
            bool moveSuccesful = AttemptMove(direction);

            if (!moveSuccesful) {
                _scoring.Land(_currentTetromino.matrix.GetNonZeroCount());

                CheckForFullRows();
                if (!AttemptSpawnNextTetromino())
                    // GameOver
                    _gameState = GameState.OVER;
            }
        }

        /// <summary>
        /// Moves the current tetromino to the left
        /// </summary>
        public void MoveLeft() {
            Move(Direction.LEFT);
        }

        /// <summary>
        /// Moves the current tetromino to the right
        /// </summary>
        public void MoveRight() {
            Move(Direction.RIGHT);
        }

        /// <summary>
        /// Moves the current tetromino down.
        /// This should only be called when user input is given, because this method resets the automatic drop timer.
        /// </summary>
        public void MoveDown() {
            Move(Direction.DOWN);

            _scoring.SoftDrop();

            // A hacky way to reset the current interval on the timer.
            // There sadly don't seem to be other ways to achieve this.
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Attempt to rotate. Tries 90 degree rotation first.
        /// If that doesn't work, tries to rotate 90 degrees counter clockwise.
        /// </summary>
        public void Rotate() {
            if (!AttemptRotate(Direction.RIGHT))
                AttemptRotate(Direction.LEFT);
        }

        /// <summary>
        /// Attempts to rotate in the given direction.
        /// </summary>
        /// <param name="direction">The direction of where to rotate, can be either <see cref="Direction"/> LEFT or RIGHT.</param>
        /// <returns>Whether the rotation was succesful.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if anything other than LEFT or RIGHT was passed as argument
        /// </exception>
        private bool AttemptRotate(Direction direction) {
            Tetromino tetrominoPreRotate = _currentTetromino;

            Matrix rotatedMatrix = direction switch {
                Direction.RIGHT => _currentTetromino.matrix.Rotate90(),
                Direction.LEFT => _currentTetromino.matrix.Rotate90CounterClockwise(),
                _ => throw new ArgumentException("AttemptRotate() must be called with either \"LEFT\" or \"RIGHT\"")
            };

            Tetromino postRotate = new Tetromino(rotatedMatrix, _currentTetromino.xPos, _currentTetromino.yPos);

            // Like the Move() method, the actions in these queues are only executed after all the checks pass.
            List<Action> deleteActionQueue = new List<Action>();
            List<Action> updateActionQueue = new List<Action>();

            for (int y = 0; y < rotatedMatrix.Value.GetLength(0); y++)
            for (int x = 0; x < rotatedMatrix.Value.GetLength(1); x++) {
                // Get the tetromino type of the current position
                int tetrominoType = rotatedMatrix.Value[y, x];

                // Get the coordinates of the current single point in the matrix
                int rotatedCoordX = postRotate.xPos + x;
                int rotatedCoordY = postRotate.yPos - (rotatedMatrix.Value.GetLength(0) - 1) + y;

                // If out of bounds on any side, don't rotate
                if (rotatedCoordX > _columns - 1 || rotatedCoordX < 0 ||
                    rotatedCoordY > _rows - 1 || rotatedCoordY < 0) {
                    return false;
                }

                // If the cell at one of the new positions is already set and
                // it is not the cells of the pre-rotation matrix, this action is not possible
                if (tetrominoType != 0 &&
                    _board.CellIsSet(rotatedCoordX, rotatedCoordY) &&
                    !tetrominoPreRotate.IsOnCoordinates(rotatedCoordX, rotatedCoordY)) {
                    return false;
                }

                // Empty the cells from the old position and rotation
                if (tetrominoPreRotate.IsOnCoordinates(rotatedCoordX, rotatedCoordY))
                    deleteActionQueue.Add(() => _board.EmptyCell(rotatedCoordX, rotatedCoordY));
                
                // Set the cell with the current type if the type isn't 0
                if (tetrominoType != 0)
                    updateActionQueue.Add(() => _board.SetCell(rotatedCoordX, rotatedCoordY, tetrominoType));
            }

            _currentTetromino = postRotate;

            foreach (Action deleteCell in deleteActionQueue)
                deleteCell();

            foreach (Action updateCellAndTetromino in updateActionQueue)
                updateCellAndTetromino();

            return true;
        }

       
        /// <summary>
        /// Attempts to spawn the next tetromino.
        /// </summary>
        /// <returns>Whether the spawn is possible</returns>
        private bool AttemptSpawnNextTetromino() {
            // Get and removes the next tetromino from the queue
            Matrix matrix = _queue.Dequeue();

            // Add a new tetromino to the queue
            QueueNewTetromino();

            // Define the starting Y position of the next tetromino
            int startingY = matrix.Value.GetLength(0) - matrix.GetFirstNonEmptyRow() - 1;

            // Create a new tetromino with the randomly picked shape and the starting position
            _currentTetromino = new Tetromino(
                matrix,
                _columns / 2 - 1,
                startingY);

            // Save the two dimensions in variables
            int matrixHeight = matrix.Value.GetLength(0);
            int matrixWidth = matrix.Value.GetLength(1);

            // Define a list of actions that need to be executed if all the coordinates are valid.
            List<Action> addActionQueue = new List<Action>();

            for (int y = 0; y < matrixHeight; y++)
            for (int x = 0; x < matrixWidth; x++) {
                // Get the type of the current position
                int tetrominoType = matrix.Value[y, x];

                // If there is a 0 at the current coordinates, skip this iteration.
                if (tetrominoType == 0)
                    continue;

                // Get the coordinates of the current single point in the matrix
                int coordX = _currentTetromino.xPos + x;
                int coordY = _currentTetromino.yPos - (matrixHeight - 1) + y;

                // If the cell at one of the new positions is already set,
                // no new piece can spawn and the game is over.
                if (_board.CellIsSet(coordX, coordY)) {
                    return false; // Don't process move and end game.
                }

                // Add the two actions to their respective queues, to be performed if none of the given cells are set.
                addActionQueue.Add(() => _board.SetCell(coordX, coordY, tetrominoType));
            }

            foreach (Action addCell in addActionQueue)
                addCell();

            return true;
        }

        /// <summary>
        /// Checks whether there are any full rows in the board.
        /// If there are, removes them, drops any rows above it and updates the score
        /// </summary>
        private void CheckForFullRows() {
            IEnumerator<int> fullRows = _board.GetCompleteRows();
            int linesCleared = 0;

            while (fullRows.MoveNext()) {
                int nextRow = fullRows.Current;

                for (int x = 0; x < _columns; x++)
                    _board.EmptyCell(x, nextRow);

                _board.DropFloatingRows(nextRow);
                linesCleared++;
            }

            if (linesCleared > 0)
                _scoring.LinesCleared(linesCleared);
        }

        /// <summary>
        /// Adds a new random tetromino to the queue.
        /// </summary>
        private void QueueNewTetromino() {
            _queue.Enqueue(Shapes.RandomShape(_random));
        }
    }
}