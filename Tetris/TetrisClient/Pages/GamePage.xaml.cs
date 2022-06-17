using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TetrisEngine;

namespace TetrisClient {
    public partial class GamePage {
        public TetrisGame Game;

        public GamePage() {
            InitializeComponent();
        }

        public void Initialize(int seed = 0) {
            // Register the event listeners for key presses.
            RegisterKeyListener(KeyPressed);

            // Create new Game object with the amount of rows and columns that is being played with
            Game = new TetrisGame(Constants.ROWS, Constants.COLUMNS, seed);

            // Initialize the game
            Game.InitializeGame();
            
            Game.AddTimerListener(UpdateTick);
            
            Game.StartGame();
            
            Update();
        }

        /// <summary>
        /// The method that handles the update task. Updates the board, queue and score.
        /// </summary>
        private void UpdateTick(object sender, EventArgs e) {
            Dispatcher.Invoke(Update);
        }

        private void Update() {
            if (Game.GameState == GameState.OVER)
                EndGame();

            GameGrid.UpdateBoard(Game.Board);
            QueueGrid.UpdateBoard(Game.Queue);
            UpdateScore();
        }

        private void EndGame() {
            GameOverScreen.Visibility = Visibility.Visible;
            RemoveKeyListener(KeyPressed);
        }

        /// <summary>
        /// Updates the score with any changes from the game engine.
        /// </summary>
        private void UpdateScore() {
            PointsLabel.Content = Game.Points;
            LinesLabel.Content = Game.Lines;
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
                    Game.MoveLeft();
                    break;
                case Key.S:
                case Key.Down:
                    Game.MoveDown();
                    break;
                case Key.D:
                case Key.Right:
                    Game.MoveRight();
                    break;
                case Key.W:
                case Key.Up:
                    if (!Game.Rotate(Direction.RIGHT))
                        Game.Rotate(Direction.LEFT);
                    break;
            }

            // Update the game board after handling an action
            Update();
        }

        /// <summary>
        /// Subscribe to the KeyDown event with the KeyPressed handler
        /// </summary>
        public void RegisterKeyListener(KeyEventHandler method) {
            var window = Window.GetWindow(this);
            window.KeyDown += method;
        }

        /// <summary>
        /// Unsubscribe from the KeyDown event with the KeyPressed handler
        /// </summary>
        public void RemoveKeyListener(KeyEventHandler method) {
            var window = Window.GetWindow(this);
            window.KeyDown -= method;
        }
    }
}