using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TetrisClient {
    /// <summary>
    /// Parent class used for creating custom grids based off of this grid.
    /// This grid is abstract and can not be instantiated. A sub class should be made.
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
        /// Updates the grid with any given changes.
        /// </summary>
        public void UpdateBoard(int[,] newGrid) {
            // Clear the grid
            ClearGrid();

            // Draw the cells from the board onto the game grid
            for (int y = 0; y < newGrid.GetLength(0); y++)
            for (int x = 0; x < newGrid.GetLength(1); x++)
                DrawCell(x, y, newGrid[y, x]);
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
                Stroke = type == -1 ? Brushes.DarkSlateGray : Brushes.Transparent, // The border
                StrokeThickness = type == -1 ? 0.5 : 2, // Thickness of the border
                Fill = Constants.ColorMap[type] // Background color
            };
    
            TetrisGrid.Children.Add(rectangle); // Add the rectangle to the grid
            SetRow(rectangle, y); // Place the row
            SetColumn(rectangle, x); // Place the column
        }

        /// <summary>
        /// Used to clear the given grid of all the rectangle objects.
        /// </summary>
        private void ClearGrid() {
            // Get all children of grid that are of type Rectangle using a LINQ Where
            List<UIElement> rectangles =
                TetrisGrid.Children.OfType<UIElement>()
                    .Where(el => el is Rectangle)
                    .ToList();

            foreach (UIElement rectangle in rectangles)
                TetrisGrid.Children.Remove(rectangle);
        }

        /// <summary>
        /// Draw the grid with the given rows, columns and cell size.
        /// Don't draw borders if ShowBorders is false
        /// </summary>
        protected void Draw() {
            for (int i = 0; i < _rows; i++) {
                TetrisGrid.RowDefinitions.Add(new RowDefinition {
                    Height = new GridLength(_size)
                });
                
                if (!ShowBorders)
                    continue;

                Border border;
                if (i == _rows - 1)
                    border = new Border {
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };
                else if (i % 2 == 0)
                    border = new Border {
                        BorderThickness = new Thickness(0, 1, 0, 1)
                    };
                else
                    continue;
                
                border.BorderBrush = new BrushConverter().ConvertFrom("#FFA8C1CF") as Brush;
                
                TetrisGrid.Children.Add(border);

                SetRow(border, i);
                SetColumnSpan(border, _columns);
            }

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