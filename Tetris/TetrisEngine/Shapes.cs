using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace TetrisEngine {
    public class Tetromino {
        public Matrix matrix { get; }
        public int xPos;
        public int yPos;

        public List<(int, int)> Coordinates => GetCoordinates();
        public int Type => GetTetrominoType();

        /// <param name="matrix">The matrix of the tetromino.</param>
        /// <param name="yPos">The highest Y coordinate of the matrix' position.</param>
        /// <param name="xPos">The lowest X coordinate of the matrix' position.</param>
        public Tetromino(Matrix matrix, int xPos, int yPos) {
            this.matrix = matrix;
            this.xPos = xPos;
            this.yPos = yPos;
        }

        public Tetromino(Tetromino toClone) : this(toClone.matrix, toClone.xPos, toClone.yPos) {
        }

        /// <summary>
        /// Used to define whether the tetromino is positioned on the given coordinates
        /// </summary>
        /// <returns>Whether this tetromino has a non-0 cell on the given coordinates</returns>
        [Pure]
        public bool IsOnCoordinates(int xCoord, int yCoord) {
            return GetCoordinates().Contains((xCoord, yCoord));
        }

        /// <summary>
        /// Gets a list of tuples that correspond to all the coordinates of this matrix.
        /// </summary>
        /// <returns>A list of this matrix' coordinates in the form of [(x, y), (x, y)]</returns>
        [Pure]
        private List<(int, int)> GetCoordinates() {
            List<(int, int)> coordinates = new List<(int, int)>();

            int matrixHeight = matrix.Value.GetLength(0);

            for (int y = 0; y < matrixHeight; y++)
            for (int x = 0; x < matrix.Value.GetLength(1); x++)
                if (matrix.Value[y, x] != 0)
                    coordinates.Add((xPos + x, yPos - (matrixHeight - 1 - y)));

            return coordinates;
        }

        [Pure]
        private int GetTetrominoType() {
            return matrix.Value.Cast<int>()
                .ToList()
                .FirstOrDefault(i => i != 0);
        }

        [Pure]
        public Tetromino AsGhost() {
            int matrixHeight = matrix.Value.GetLength(0);
            int matrixWidth = matrix.Value.GetLength(1);

            int[,] newIntArr = new int[matrixHeight, matrixWidth];
            Matrix newMatrix = new Matrix(newIntArr);

            for (int y = 0; y < matrixHeight; y++)
            for (int x = 0; x < matrixWidth; x++)
                if (matrix.Value[y, x] != 0)
                    newIntArr[y, x] = -1;

            return new Tetromino(newMatrix, xPos, yPos);
        }
    }

    public static class Shapes {
        /// Shape reference (https://tetris.fandom.com/wiki/Tetromino)
        /// 
        /// <summary>
        /// A list of Shapes used in Tetris. They are represented as Matrixes.
        /// Shapes have a corresponding number. This is used to be able to identify different shapes on a board. <br/>
        /// I: 1
        /// J: 2
        /// L: 3
        /// O: 4
        /// S: 5
        /// T: 6
        /// Z: 7
        /// </summary>
        private static readonly List<Matrix> shapes = new() {
            // I  ▀▀▀▀ 
            new Matrix(new int[,] {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            }),


            // J  █▄▄
            new Matrix(new int[,] {
                { 2, 0, 0 },
                { 2, 2, 2 },
                { 0, 0, 0 }
            }),

            // L  ▄▄█
            new Matrix(new int[,] {
                { 0, 0, 3 },
                { 3, 3, 3 },
                { 0, 0, 0 }
            }),

            // O  ██
            new Matrix(new int[,] {
                { 4, 4 },
                { 4, 4 }
            }),

            // S  ▄█▀
            new Matrix(new int[,] {
                { 0, 5, 5 },
                { 5, 5, 0 },
                { 0, 0, 0 }
            }),

            // T  ▄█▄
            new Matrix(new int[,] {
                { 0, 6, 0 },
                { 6, 6, 6 },
                { 0, 0, 0 }
            }),

            // Z  ▀█▄
            new Matrix(new int[,] {
                { 7, 7, 0 },
                { 0, 7, 7 },
                { 0, 0, 0 }
            })
        };

        /// <summary>
        ///  Chooses a random shape from the list of pre-defined Tetris shapes
        /// </summary>
        /// <returns> A random shape in the form of a Matrix </returns>
        public static Matrix RandomShape(Random random) {
            return shapes[random.Next(shapes.Count)];
        }
    }
}