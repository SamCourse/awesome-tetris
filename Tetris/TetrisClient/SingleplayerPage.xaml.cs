using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TetrisEngine;

namespace TetrisClient {
    public partial class SingleplayerPage : Page {
        private DispatcherTimer _timer;
        private TetrisGame _game;

        public SingleplayerPage() {
            InitializeComponent();
        }

        public void Initiate() {
            // Register the event listeners for key presses.
            RegisterKeyEventListener();

            // Create new Game object with the amount of rows and columns that is being played with
            _game = new TetrisGame(Constants.ROWS, Constants.COLUMNS);

            // Initialize the game
            _game.InitializeGame();

            // Starts the loop for updating the UI
            StartUpdateBoardTask();
        }

        /// <summary>
        /// Starts the updating task.
        /// </summary>
        private void StartUpdateBoardTask() {
            _timer = new DispatcherTimer();
            _timer.Tick += UpdateTick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 10); // Update every 10 ms.
            _timer.Start();
        }

        /// <summary>
        /// The method that handles the update task. Updates the board, queue and score.
        /// </summary>
        private void UpdateTick(object sender, EventArgs e) {
            GameGrid.UpdateBoard(_game);
            QueueGrid.UpdateBoard(_game);
            UpdateScore();
        }

        /// <summary>
        /// Updates the score with any changes from the game engine.
        /// </summary>
        private void UpdateScore() {
            PointsLabel.Content = _game.Points;
            LinesLabel.Content = _game.Lines;
        }
        
        /// <summary>
        /// The event handler that handles key presses. Only up-down-left-right and ASDW are handled here.
        /// </summary>
        private void KeyPressed(object sender, KeyEventArgs keyPress) {
            // Handle key presses
            switch (keyPress.Key) {
                default:
                    return;
                case Key.A:
                case Key.Left:
                    _game.MoveLeft();
                    break;
                case Key.S:
                case Key.Down:
                    _game.MoveDown();
                    break;
                case Key.D:
                case Key.Right:
                    _game.MoveRight();
                    break;
                case Key.W:
                case Key.Up:
                    _game.Rotate();
                    break;
            }

            keyPress.Handled = true;

            // Update the game board after handling an action
            GameGrid.UpdateBoard(_game);
        }

        /// <summary>
        /// Subscribe to the KeyDown event with the KeyPressed handler
        /// </summary>
        private void RegisterKeyEventListener() {
            KeyDown += KeyPressed;
        }

        /// <summary>
        /// Unsubscribe from the KeyDown event with the KeyPressed handler
        /// </summary>
        private void UnregisterKeyEventListener() {
            KeyDown -= KeyPressed;
        }
    }
}