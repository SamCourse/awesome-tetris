using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TetrisEngine;
using Matrix = TetrisEngine.Matrix;

namespace TetrisClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private DispatcherTimer _timer;
        private TetrisGame _game;

        public MainWindow() {
            InitializeComponent();
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
            UpdateBoard();
            UpdateQueue();
            UpdateScore();
        }

        /// <summary>
        /// Updates the UI board with any changes from the game engine.
        /// </summary>
        private void UpdateBoard() {
            // Clear the game grid
            ClearGrid(TetrisGrid);
            int[,] board = _game.Board;
            
            // Draw the cells from the board onto the game grid
            for (int y = 0; y < board.GetLength(0); y++)
                for (int x = 0; x < board.GetLength(1); x++)
                    DrawCell(x, y, board[y, x]);
        }

        /// <summary>
        /// Updates the UI queue with any changes from the game engine.
        /// </summary>
        private void UpdateQueue() {
            // Clear the queue grid
            ClearGrid(QueueGrid);
            
            // Get the list of matrices that are in the queue from the engine
            List<Matrix> queue = _game.Queue.ToList();

            for (int i = 0; i < queue.Count; i++) {
                int shapeYStartPosition = i * 3; // Every shape will be positioned 3 cells lower than the previous.
                int[,] matrix = queue[i].Value;

                // Draw the queue cells in the queue grid
                for (int y = 0; y < matrix.GetLength(0); y++)
                    for (int x = 0; x < matrix.GetLength(1); x++)
                        DrawQueueCell(x, y + shapeYStartPosition, matrix[y, x]);
                
            }
        }

        /// <summary>
        /// Updates the score with any changes from the game engine.
        /// </summary>
        private void UpdateScore() {
            PointsLabel.Content = _game.Points;
            LinesLabel.Content = _game.Lines;
        }

        /// <summary>
        /// Draws a rectangle in the queue grid.
        /// </summary>
        /// <param name="x">The x position of what cell to draw</param>
        /// <param name="y">The y position of what cell to draw</param>
        /// <param name="type">The type of the cell that needs to be drawn</param>
        private void DrawQueueCell(int x, int y, int type) {
            Rectangle rectangle = new Rectangle {
                Width = 25, // Width of a cell in the grid
                Height = 25, // Height of a cell in the grid
                Stroke = Brushes.Transparent, // The border
                StrokeThickness = 1, // Thickness of the border
                Fill = Constants.ColorMap[type] // Background color
            };

            QueueGrid.Children.Add(rectangle); // Add the rectangle to the grid
            Grid.SetRow(rectangle, y); // Place the row
            Grid.SetColumn(rectangle, x); // Place the column
        }

        /// <summary>
        /// Draw a rectangle on the game grid at the given coordinates
        /// </summary>
        /// <param name="x">The x coordinate of the cell</param>
        /// <param name="y">The y coordinate of the cell</param>
        /// <param name="type">The type of tetromino, see <see cref="Constants.ColorMap"/> and <see cref="Shapes.shapes"/>.</param>
        private void DrawCell(int x, int y, int type) {
            Rectangle rectangle = new Rectangle {
                Width = 30, // Width of a cell in the grid
                Height = 30, // Height of a cell in the grid
                Stroke = Brushes.Transparent, // The border
                StrokeThickness = 1, // Thickness of the border
                Fill = Constants.ColorMap[type] // Background color
            };

            TetrisGrid.Children.Add(rectangle); // Add the rectangle to the grid
            Grid.SetRow(rectangle, y); // Place the row
            Grid.SetColumn(rectangle, x); // Place the column
        }


        /// <summary>
        /// Used to clear the given grid of all the rectangle objects.
        /// </summary>
        /// <param name="grid">The grid that needs to be cleared</param>
        private static void ClearGrid(Grid grid) {
            // Get all children of grid that are of type Rectangle using a LINQ Where
            List<UIElement> rectangles =
                grid.Children.OfType<UIElement>()
                    .Where(el => el is Rectangle)
                    .ToList();

            foreach (UIElement rectangle in rectangles)
                grid.Children.Remove(rectangle);
            
        }

        /// <summary>
        /// Start button click handler.
        /// When the start button is clicked, the game is initialized.
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e) {
            // Hide the start game modal
            StartModal.Visibility = Visibility.Hidden;

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
        /// Opens a WikiHow page on how to play Tetris
        /// </summary>
        private void HowToButton_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = "https://www.wikihow.com/Play-Tetris",
                UseShellExecute = true
            });
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

            // Update the board after handling an action
            UpdateBoard();
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