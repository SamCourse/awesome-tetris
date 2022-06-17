using System.Collections.Generic;
using System.Windows.Media;

namespace TetrisClient {
    public static class Constants {
        /// <summary>
        /// The amount of Rows and Columns used for the game
        /// </summary>
        public const int ROWS = 16;

        public const int COLUMNS = 10;

        /// <summary>
        /// A Dictionary of tetris shape number as key, with the corresponding color as the value.
        /// </summary>
        public static readonly Dictionary<int, SolidColorBrush> ColorMap = new() {
            { -1, Brushes.Transparent }, // Ghost piece is transparent, but has borders.
            { 0, Brushes.Transparent },
            { 1, Brushes.Aqua },
            { 2, Brushes.Blue },
            { 3, Brushes.Orange },
            { 4, Brushes.Yellow },
            { 5, Brushes.Lime },
            { 6, Brushes.Purple },
            { 7, Brushes.Red }
        };
    }
}