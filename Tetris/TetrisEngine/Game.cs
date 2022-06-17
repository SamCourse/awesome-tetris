using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;

namespace TetrisEngine {
    public enum Direction {
        LEFT,
        DOWN,
        RIGHT
    }

    public enum GameState {
        CREATED,
        PLAYING,
        PAUSED, // TODO
        OVER
    }

    public class TetrisGame {
        private readonly Queue<Matrix> _queue;
        private Tetromino _currentTetromino;
        private Board _board;
        private Timer _timer;
        private Scoring _scoring;
        public GameState GameState;
        private Random _random;
        

        public int[,] Board => _board._board;
        public int Points => _scoring.Points;
        public int Lines => _scoring.Lines;
        public int[,] Queue => Utils.QueueToIntArr(_queue);

        public TetrisGame(int rows, int columns, int seed) {
            _queue = new Queue<Matrix>();
            _board = new Board(rows, columns);
            GameState = GameState.CREATED;
            _random = seed == 0 ? new Random() : new Random(seed);
        }

        /// <summary>
        /// Initializes the game. Sets the gamestate and fills the tetromino queue.
        /// </summary>
        public void InitializeGame() {
            // Initialize the scoring system with the Normal-mode scoring system.
            _scoring = Scoring.NormalGame();

            // Set the gamestate to playing.
            GameState = GameState.PLAYING;

            // Queue 3 tetromino's
            for (int i = 0; i < 3; i++) {
                QueueNewTetromino();
            }
            
            SetupFallTimer();
        }

        public void AddTimerListener(ElapsedEventHandler method) {
            if (_timer == null)
                return;
            
            _timer.Elapsed += method;
        }


        /// <summary>
        /// Spawns the first tetromino, and sets up the automatic falling timer.
        /// </summary>
        public void StartGame() {
            AttemptSpawnNext();
            _timer.Start();
        }

        /// <summary>
        /// Sets up the timer that handles the tetromino being dropped every x milliseconds.
        /// </summary>
        private void SetupFallTimer() {
            _timer = new Timer(1000);

            // Register the event for when a piece falls
            AddTimerListener(TimerTick);
        }

        private void TimerTick(object sender, EventArgs e) {
            AttemptMove(Direction.DOWN);
            _scoring.Fall();
        }

        private void Move(int xOffset, int yOffset) {
            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y);
            });

            _currentTetromino.xPos += xOffset;
            _currentTetromino.yPos += yOffset;
            
            if (xOffset != 0)
                PlaceGhostPiece();
            
            Place(_currentTetromino);
        }

        private void Place(Tetromino tetromino) {
            tetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.SetCell(x, y, tetromino.Type);
            });
        }

        /// <summary>
        /// Tries to move the current tetromino.
        /// If unsuccesful, updates the score and checks for any completed rows. Then tries to spawn the next tetromino.
        /// If the spawn was unsuccesful, ends the game.
        /// </summary>
        /// <param name="direction">The direction the move is in.</param>
        private bool AttemptMove(Direction direction) {
            (int offsetX, int offsetY) = direction switch {
                Direction.LEFT => (-1, 0),
                Direction.RIGHT => (1, 0),
                Direction.DOWN => (0, 1)
            };

            Tetromino tempTetromino = new Tetromino(_currentTetromino.matrix,
                _currentTetromino.xPos + offsetX,
                _currentTetromino.yPos + offsetY);

            bool canMove = _board.CanPlace(tempTetromino, _currentTetromino);

            if (canMove) {
                Move(offsetX, offsetY);
                return true;
            }
            if (direction == Direction.DOWN) {
                _scoring.Land(_currentTetromino.matrix.GetNonZeroCount());

                CheckForFullRows();
                if (!AttemptSpawnNext())
                    EndGame();
            }

            return false;
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
        public bool MoveDown() {
            if (AttemptMove(Direction.DOWN)) {
                _scoring.SoftDrop();

                // A hacky way to reset the current interval on the timer.
                // There doesn't seem to be another way of achieving this.
                _timer.Stop();
                _timer.Start();

                return true;
            }

            return false;
        }

        public void MoveDownHard() {
            while (MoveDown()) { }
        }

        public bool Rotate(Direction direction) {
            Matrix rotatedMatrix = direction switch {
                Direction.RIGHT => _currentTetromino.matrix.Rotate90(),
                Direction.LEFT => _currentTetromino.matrix.Rotate90CounterClockwise(),
                _ => throw new ArgumentException("AttemptRotate() must be called with either \"LEFT\" or \"RIGHT\"")
            };

            Tetromino tetrominoRotated = new Tetromino(rotatedMatrix, _currentTetromino.xPos, _currentTetromino.yPos);

            if (!_board.CanPlace(tetrominoRotated, _currentTetromino))
                return false;

            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y);
            });
            
            Place(tetrominoRotated);
            
            _currentTetromino = tetrominoRotated;
            
            PlaceGhostPiece();

            return true;
        }

        private void PlaceGhostPiece() {
            _board.RemoveGhostPiece();
            
            Tetromino ghostPiece = _currentTetromino.AsGhost();

            while (_board.CanPlace(ghostPiece, _currentTetromino)) {
                ghostPiece.yPos++;
            }

            ghostPiece.yPos--;
            
            Place(ghostPiece);
        }

        /// <summary>
        /// Attempts to spawn the next tetromino.
        /// </summary>
        /// <returns>Whether the spawn is possible</returns>
        private bool AttemptSpawnNext() {
            // Get and removes the next tetromino from the queue
            Matrix matrix = _queue.Peek();

            // Define the starting Y position of the next tetromino
            int startingY = matrix.Value.GetLength(0) - matrix.GetFirstNonEmptyRow() - 1;

            // Create a new tetromino with the randomly picked shape and the starting position
            _currentTetromino = new Tetromino(
                matrix,
                0,
                startingY);

            if (_board.CanSpawnNew(_currentTetromino)) {
                // Add a new tetromino to the queue
                QueueNewTetromino();
                _queue.Dequeue();
                PlaceGhostPiece();
                _board.SpawnNew(_currentTetromino);
                
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

                for (int x = 0; x < Board.GetLength(1); x++)
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

        private void EndGame() {
            GameState = GameState.OVER;
            _timer.Stop();
        }
    }
}