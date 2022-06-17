using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TetrisClient {
    /// <summary>
    /// Abstract xustom grid used for creating custom grids.
    /// This grid is abstract and thus can not be instantiated. A sub class should be made.
    /// </summary>
    public abstract partial class CustomGrid {
        private int _rows;
        private int _columns;
        private int _size;

        protected bool ShowBorders = true;

        protected CustomGrid(int rows, int columns, int size) {
            _rows = rows;
            _columns = columns;
            _size = size;

            InitializeComponent();
        }

        /// <summary>
        /// Updates the grid with the passed new grid
        /// </summary>
        public void UpdateBoard(int[,] grid) {
            // Clear the grid before drawing a new one
            ClearGrid();

            // Draw the cells from the board onto the game grid
            for (int y = 0; y < grid.GetLength(0); y++)
            for (int x = 0; x < grid.GetLength(1); x++)
                DrawCell(x, y, grid[y, x]);
        }

        /// <summary>
        /// Draw a rectangle on the grid at the given coordinates
        /// </summary>
        /// <param name="x">The x coordinate of the cell</param>
        /// <param name="y">The y coordinate of the cell</param>
        /// <param name="type">The type of cell, see <see cref="Constants.ColorMap"/>.</param>
        private void DrawCell(int x, int y, int type) {
            Rectangle rectangle = new Rectangle {
                Width = _size, // Width of a cell in the grid
                Height = _size, // Height of a cell in the grid
                Stroke = type == -1 ? Brushes.DarkSlateGray : Brushes.Transparent, // The border. Transparent if
                // it is not a ghost piece.
                StrokeThickness = type == -1 ? 0.5 : 2, // Thickness of the border. Thicker if not a ghost piece.
                Fill = Constants.ColorMap[type] // Background color
            };

            TetrisGrid.Children.Add(rectangle); // Add the rectangle to the grid
            SetRow(rectangle, y); // Place the row
            SetColumn(rectangle, x); // Place the column
        }

        /// <summary>
        /// Used to clear the given grid of all the rectangle objects it has as children.
        /// </summary>
        private void ClearGrid() {
            // Get all children of grid that are of type Rectangle
            List<UIElement> rectangles =
                TetrisGrid.Children.OfType<UIElement>()
                    .Where(el => el is Rectangle)
                    .ToList();

            // Iterate over the list and remove every element as a child from the grid
            foreach (UIElement rectangle in rectangles)
                TetrisGrid.Children.Remove(rectangle);
        }

        /// <summary>
        /// Draw the grid with borders with the given row and column count in the constructor, and the size of the cells.
        /// </summary>
        protected void Draw() {
            // Iterate over all the rows to draw the borders for the cell
            for (int i = 0; i < _rows; i++) {
                // Add a row to the grid with the given size 
                TetrisGrid.RowDefinitions.Add(new RowDefinition {
                    Height = new GridLength(_size)
                });

                // If showborders is set to false, don't show the borders. E.G. for the queue.
                if (!ShowBorders)
                    continue;

                Border border;
                if (i == _rows - 1) // If it is the last row, draw only the bottom cell border.
                    border = new Border {
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };

                else if (i % 2 == 0) // Every other cell should have a border on both top and bottom
                    border = new Border {
                        BorderThickness = new Thickness(0, 1, 0, 1)
                    };
                else // If not last row or not an even row number, skip this iteration 
                    continue;

                // Set the border color
                border.BorderBrush = new BrushConverter().ConvertFrom("#FFA8C1CF") as Brush;

                // Add the border to the grid children
                TetrisGrid.Children.Add(border);

                // Add the border to the given row
                SetRow(border, i);
                // Add the border to the entire column
                SetColumnSpan(border, _columns);
            }

            // Exact same as previous loop but for the columns instead of the rows.
            for (int i = 0; i < _columns; i++) {
                TetrisGrid.ColumnDefinitions.Add(new ColumnDefinition {
                    Width = new GridLength(_size)
                });

                if (!ShowBorders)
                    continue;

                Border border;
                if (i == _columns - 1)
                    border = new Border {
                        BorderThickness = new Thickness(0, 0, 1, 0)
                    };
                else if (i % 2 == 0)
                    border = new Border {
                        BorderThickness = new Thickness(1, 0, 1, 0)
                    };
                else
                    continue;

                border.BorderBrush = new BrushConverter().ConvertFrom("#FFA8C1CF") as Brush;

                TetrisGrid.Children.Add(border);

                SetColumn(border, i);
                SetRowSpan(border, _rows);
            }
        }
    }
}