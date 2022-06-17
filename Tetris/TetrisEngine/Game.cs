using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Xml.XPath;

namespace TetrisEngine {
    public enum Direction {
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
                AttemptMove(Direction.DOWN);
                _scoring.Fall();
            };

            _timer.Start();
        }

        private void Move(int xOffset, int yOffset) {
            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y);
            });

            _currentTetromino.xPos += xOffset;
            _currentTetromino.yPos += yOffset;

            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.SetCell(x, y, _currentTetromino.Type);
            });
        }

        /// <summary>
        /// Tries to move the current tetromino.
        /// If unsuccesful, updates the score and checks for any completed rows. Then tries to spawn the next tetromino.
        /// If the spawn was unsuccesful, ends the game.
        /// </summary>
        /// <param name="direction">The direction the move is in.</param>
        private void AttemptMove(Direction direction) {
            (int offsetX, int offsetY) = direction switch {
                Direction.LEFT => (-1, 0),
                Direction.RIGHT => (1, 0),
                Direction.DOWN => (0, 1)
            };

            Tetromino tempTetromino = new Tetromino(_currentTetromino.matrix,
                _currentTetromino.xPos + offsetX,
                _currentTetromino.yPos + offsetY);

            bool canMove = _board.CanPlace(tempTetromino, _currentTetromino);

            if (canMove)
                Move(offsetX, offsetY);
            else if (direction == Direction.DOWN) {
                _scoring.Land(_currentTetromino.matrix.GetNonZeroCount());

                CheckForFullRows();
                if (!AttemptSpawnNextTetromino())
                    // TODO: GameOver
                    _gameState = GameState.OVER;
            }
        }

        /// <summary>
        /// Moves the current tetromino to the left
        /// </summary>
        public void MoveLeft() {
            AttemptMove(Direction.LEFT);
        }

        /// <summary>
        /// Moves the current tetromino to the right
        /// </summary>
        public void MoveRight() {
            AttemptMove(Direction.RIGHT);
        }

        /// <summary>
        /// Moves the current tetromino down.
        /// This should only be called when user input is given, because this method resets the automatic drop timer.
        /// </summary>
        public void MoveDown() {
            AttemptMove(Direction.DOWN);

            _scoring.SoftDrop();

            // A hacky way to reset the current interval on the timer.
            // There sadly don't seem to be other ways to achieve this.
            _timer.Stop();
            _timer.Start();
        }

        private void PerformRotate(Tetromino tetrominoRotated) {
            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y);
            });

            tetrominoRotated.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.SetCell(x, y, tetrominoRotated.Type);
            });

            _currentTetromino = tetrominoRotated;
        }

        /// <summary>
        /// Attempts to rotate in the given direction.
        /// </summary>
        /// <param name="direction">The direction of where to rotate, can be either <see cref="Direction"/> LEFT or RIGHT.</param>
        /// <returns>Whether the rotation was succesful.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if anything other than LEFT or RIGHT was passed as argument
        /// </exception>
        public bool Rotate(Direction direction) {
            Matrix rotatedMatrix = direction switch {
                Direction.RIGHT => _currentTetromino.matrix.Rotate90(),
                Direction.LEFT => _currentTetromino.matrix.Rotate90CounterClockwise(),
                _ => throw new ArgumentException("AttemptRotate() must be called with either \"LEFT\" or \"RIGHT\"")
            };

            Tetromino tetrominoRotated = new Tetromino(rotatedMatrix, _currentTetromino.xPos, _currentTetromino.yPos);

            if (_board.CanPlace(tetrominoRotated, _currentTetromino)) {
                PerformRotate(tetrominoRotated);
                return true;
            }

            return false;
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

            if (_board.CanPlace(_currentTetromino)) {
                Move(0, 0);
                return true;
            }

            return false;
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