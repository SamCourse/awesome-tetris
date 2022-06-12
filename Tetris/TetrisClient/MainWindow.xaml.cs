using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TetrisEngine;

namespace TetrisClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static Dictionary<int, SolidColorBrush> colorMap = new() {
            { 1, Brushes.Aqua },
            { 2, Brushes.Blue },
            { 3, Brushes.Orange },
            { 4, Brushes.Yellow },
            { 5, Brushes.Lime },
            { 6, Brushes.Purple },
            { 7, Brushes.Red }
        };

        private TetrisGame _game;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        public void UpdateBoard() {
            int[,] board = _game.CurrentBoard();
            
            

        /// <summary>
        /// Draw a point at the given coordinates
        /// </summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        /// <param name="type">The type of tetromino</param>
        private void DrawPoint(int x, int y, int type) {
            Rectangle rectangle = new Rectangle
            {
                Width = 25, // Width of a cell in the grid
                Height = 25, // Height of a cell in the grid
                Stroke = Brushes.Transparent, // The border
                StrokeThickness = 1, // Thickness of the border
                Fill = colorMap[type], // Background color
            };

            TetrisGrid.Children.Add(rectangle); // Add the rectangle to the grid
            Grid.SetRow(rectangle, y); // Place the row
            Grid.SetColumn(rectangle, x); // Place the column
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) {
            // Hide the start game modal
            StartModal.Visibility = Visibility.Hidden;
            
            // Create new Game object with the amount of rows and columns that is being played with
            int columns = TetrisGrid.ColumnDefinitions.Count;
            int rows = TetrisGrid.RowDefinitions.Count;
            _game = new TetrisGame(rows, columns);
            
            // Start game loop
            _game.InitializeGame();
        }
        
        
        private void HowToButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a wikihow page on how to play Tetris.
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.wikihow.com/Play-Tetris",
                UseShellExecute = true
            });
        }

        private void KeyPressed(object sender, KeyEventArgs keyPress) {
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
            
            
        }
        
        private void RegisterKeyEventListener() {
            KeyDown += KeyPressed;
        }

        private void UnregisterKeyEventListener() {
            KeyDown -= KeyPressed;
        }
    }
}
