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
        private DispatcherTimer Timer;
        private TetrisGame _game;

        public MainWindow() {
            InitializeComponent();
        }

        private void StartUpdateBoardTask() {
            Timer = new DispatcherTimer();
            Timer.Tick += dispatcherTimer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 10); // Update every 10 ms.
            Timer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            UpdateBoard();
            UpdateGrid();
        }

        private void UpdateBoard() {
            ClearGrid(TetrisGrid);

            int[,] board = _game.CurrentBoard();

            for (int y = 0; y < board.GetLength(0); y++) {
                for (int x = 0; x < board.GetLength(1); x++) {
                    int type = board[y, x];
                    DrawCell(x, y, type);
                }
            }
        }

        private void UpdateGrid() {
            ClearGrid(QueueGrid);

            List<Matrix> queue = _game._queue;

            for (int i = 0; i < queue.Count; i++) {
                int shapeYStartPosition = i * 3; // Every shape will be positioned
                                                 // 3 Y coordinates lower than the previous.
                                                 
                int[,] matrix = queue[i].Value;

                for (int y = 0; y < matrix.GetLength(0); y++) {
                    for (int x = 0; x < matrix.GetLength(1); x++) {
                        int type = matrix[y, x];

                        DrawQueueCell(x, y + shapeYStartPosition, type);
                    }
                }
            }
        }

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
        /// Draw a rectangle at the given coordinates
        /// </summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
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

            foreach (UIElement rectangle in rectangles) {
                grid.Children.Remove(rectangle);
            }
        }

        /// <summary>
        /// Start button click handler.
        /// When the start button is clicked, the game is initialized.
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e) {
            // Hide the start game modal
            StartModal.Visibility = Visibility.Hidden;

            RegisterKeyEventListener();

            // Create new Game object with the amount of rows and columns that is being played with
            _game = new TetrisGame(Constants.ROWS, Constants.COLUMNS);

            // Start game loop
            _game.InitializeGame();

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
        public void KeyPressed(object sender, KeyEventArgs keyPress) {
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

            UpdateBoard();
        }

        private void RegisterKeyEventListener() {
            KeyDown += KeyPressed;
        }

        private void UnregisterKeyEventListener() {
            KeyDown -= KeyPressed;
        }
    }
}