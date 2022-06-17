using System;
using System.Collections.Generic;
using System.Timers;

namespace TetrisEngine {
    /// <summary>
    /// Enum used for specifying directions for rotations and move actions.
    /// </summary>
    public enum Direction {
        LEFT,
        DOWN,
        RIGHT
    }

    /// <summary>
    /// The enum used for specifying the current state of the game.
    /// </summary>
    public enum GameState {
        CREATED,
        PLAYING,
        OVER
    }

    /// <summary>
    /// The tetris game main object. Controls and delegates most game-related tasks.
    /// </summary>
    public class TetrisGame {
        private const int STANDARD_INTERVAL = 1000; // The standard interval for the game timer.

        private readonly Queue<Matrix> _queue; // The queue of upcoming matrices.
        private Tetromino _currentTetromino; // The current tetromino that is being played with.
        private Board _board; // The board representation of the game.
        private Timer _timer; // The timer which is used for automatic tetromino falling
        private Scoring _scoring; // The score object representing the score of this game
        public GameState GameState; // The state of the game
        private Random _random; // The random object which determines the next matrices in the queue.


        public int[,] Board => _board._board;
        public int Points => _scoring.Points;
        public int Lines => _scoring.Lines;
        public int[,] Queue => Utils.QueueToIntArr(_queue);

        public TetrisGame(int rows, int columns, int seed) {
            _queue = new Queue<Matrix>();
            _board = new Board(rows, columns);
            GameState = GameState.CREATED;
            _random = seed == 0 ? new Random() : new Random(seed); // If the seed passed is 0,
            // don't specify a seed for the random object.
        }

        /// <summary>
        /// Initializes the game. Creates a new scoring object, sets the gamestate to playing,
        /// queues 3 new tetromino's and prepares the automatic fall timer.
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

            // Prepare the timer for automatic tetromino falling.
            SetupFallTimer();
        }

        /// <summary>
        ///  Add a listener to the timer elapsed callback.
        /// </summary>
        /// <param name="method">The event handler for the time elapsed callback</param>
        public void AddTimerListener(ElapsedEventHandler method) {
            if (_timer == null) // If timer isn't initialized, return.
                return;

            _timer.Elapsed += method;
        }


        /// <summary>
        /// Spawns the first tetromino and starts the timer.
        /// </summary>
        public void StartGame() {
            AttemptSpawnNext();
            _timer.Start();
        }

        /// <summary>
        /// Sets up the timer that handles the tetromino being dropped every x milliseconds.
        /// </summary>
        private void SetupFallTimer() {
            _timer = new Timer(STANDARD_INTERVAL);

            // Register the event for when the timer interval is hit.
            AddTimerListener(TimerTick);
        }

        /// <summary>
        /// The event handler for the timer's ticks.
        /// Tries to let the current tetromino fall down, and updates the score.
        /// </summary>
        private void TimerTick(object sender, EventArgs e) {
            AttemptMove(Direction.DOWN);
            _scoring.Fall();
        }

        /// <summary>
        /// The method used for moving the current tetromino with the given offset
        /// </summary>
        private void Move(int xOffset, int yOffset) {
            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y); // Remove the old tetromino from the board
                // before placing it in it's new position.
            });

            // Update the tetromino's position
            _currentTetromino.xPos += xOffset;
            _currentTetromino.yPos += yOffset;

            // If the tetromino moved left or right, update the ghost piece
            if (xOffset != 0)
                PlaceGhostPiece();

            // Place the tetromino and it's new position.
            Place(_currentTetromino);
        }

        /// <summary>
        /// Used to place the tetromino at it's coordinates.
        /// </summary>
        /// <param name="tetromino">The tetromino that should be placed.</param>
        private void Place(Tetromino tetromino) {
            tetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.SetCell(x, y, tetromino.Type);
            });
        }

        /// <summary>
        /// Checks whether the current tetromino can move in the given direction.
        /// If not possible and the move was down, updates the score and checks for any completed rows.
        /// Then tries to spawn the next tetromino.
        /// If not possible and the move was to the side, do nothing.
        /// If the spawn was unsuccesful, ends the game.
        /// </summary>
        /// <param name="direction">The direction the move is in.</param>
        private bool AttemptMove(Direction direction) {
            // Define the offsets determined by the direction passed
            (int offsetX, int offsetY) = direction switch {
                Direction.LEFT => (-1, 0),
                Direction.RIGHT => (1, 0),
                Direction.DOWN => (0, 1)
            };

            // Create a copy of the current tetromino
            Tetromino tempTetromino = new Tetromino(_currentTetromino.matrix,
                _currentTetromino.xPos + offsetX,
                _currentTetromino.yPos + offsetY);

            bool canMove = _board.CanPlace(tempTetromino, _currentTetromino);

            if (canMove) {
                Move(offsetX, offsetY); // Move the tetromino if possible
                return true;
            }

            if (direction == Direction.DOWN) { // If not possible and the direction was down,
                // place the tetromino down permanently. And update the score.
                // If necessary ends the game.
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

        /// <summary>
        /// The method used to do a hard drop.
        /// </summary>
        public void MoveDownHard() {
            while (MoveDown()) {
            }
        }

        /// <summary>
        /// Rotate the current matrix in the given direction.
        /// </summary>
        /// <param name="direction">The direction to move the tetromino in.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool Rotate(Direction direction) {
            // Rotates the matrix using the given direction.
            Matrix rotatedMatrix = direction switch {
                Direction.RIGHT => _currentTetromino.matrix.Rotate90(),
                Direction.LEFT => _currentTetromino.matrix.Rotate90CounterClockwise(),
                _ => throw new ArgumentException("AttemptRotate() must be called with either \"LEFT\" or \"RIGHT\"")
            };

            Tetromino tetrominoRotated = new Tetromino(rotatedMatrix, _currentTetromino.xPos, _currentTetromino.yPos);

            // If the rotated tetromino can not be placed, don't do anything.
            if (!_board.CanPlace(tetrominoRotated, _currentTetromino))
                return false;

            // Remove the old tetromino
            _currentTetromino.Coordinates.ForEach(coordinate => {
                (int x, int y) = coordinate;
                _board.EmptyCell(x, y);
            });

            _currentTetromino = tetrominoRotated;

            // Update the ghost piece
            PlaceGhostPiece();

            // Place the rotated tetromino
            Place(tetrominoRotated);

            return true;
        }


        /// <summary>
        /// Place the ghost piece based off of the position and possibilities of the current tetromino.
        /// </summary>
        private void PlaceGhostPiece() {
            // Remove the ghost piece from the board.
            _board.RemoveGhostPiece();

            // Create a ghost piece copy of the current tetromino.
            Tetromino ghostPiece = _currentTetromino.AsGhost();

            // While possible, move the ghost piece down.
            while (_board.CanPlace(ghostPiece, _currentTetromino)) {
                ghostPiece.yPos++;
            }

            // Set the ghost piece's Y pos down by once, because last while() call it wasn't possible.
            ghostPiece.yPos--;

            // Place the ghost piece on the board.
            Place(ghostPiece);
        }

        /// <summary>
        /// Attempts to spawn the next tetromino.
        /// </summary>
        /// <returns>Whether the spawn is possible</returns>
        private bool AttemptSpawnNext() {
            // Peek the next matrix to check whether the spawn is possible.
            Matrix matrix = _queue.Peek();

            // Define the starting Y position of the next tetromino
            int startingY = matrix.Value.GetLength(0) - matrix.GetFirstNonEmptyRow() - 1;

            // Create a new tetromino with the randomly picked shape and the starting position
            _currentTetromino = new Tetromino(
                matrix,
                0,
                startingY);

            if (_board.CanSpawnNew(_currentTetromino)) { // If the tetromino can spanw
                // Add a new tetromino to the queue
                QueueNewTetromino();
                // Remove the matrix from the queue
                _queue.Dequeue();
                // Update the ghost piece
                PlaceGhostPiece();
                // Spawn the new tetromino
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

            // Drop rows while there are any full rows in the returned list of rows.
            while (fullRows.MoveNext()) {
                int nextRow = fullRows.Current;

                for (int x = 0; x < Board.GetLength(1); x++)
                    _board.EmptyCell(x, nextRow);

                _board.DropFloatingRows(nextRow);
                linesCleared++;
            }

            if (linesCleared > 0) { // If there were any lines cleared, update the score and make the timer faster.
                _scoring.LinesCleared(linesCleared);
                ReduceIntervalCheck();
            }
        }

        /// <summary>
        /// Used to check whether the timer needs to be faster, and sets it faster if needed.
        /// </summary>
        private void ReduceIntervalCheck() {
            if (_scoring.Lines > 5) {
                int lines5Amount = _scoring.Lines / 5;
                // Every 5 lines scored, the timer is set 0.75 ^(lines / 5) faster. 
                _timer.Interval = Convert.ToDouble(STANDARD_INTERVAL * Math.Pow(0.75, lines5Amount));
            }
        }

        /// <summary>
        /// Adds a new random tetromino to the queue.
        /// </summary>
        private void QueueNewTetromino() {
            _queue.Enqueue(Shapes.RandomShape(_random));
        }

        /// <summary>
        /// Ends the game, sets the gamestate to over and stop the timer.
        /// </summary>
        private void EndGame() {
            GameState = GameState.OVER;
            _timer.Stop();
        }
    }
}