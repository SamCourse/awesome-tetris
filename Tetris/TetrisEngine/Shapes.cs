using System;
using System.Collections.Generic;

namespace TetrisEngine {
    public class Tetromino {
        public Matrix matrix { get; }
        public int xPos { get; set; }
        public int yPos { get; set; }


        /// <param name="matrix">The matrix of the tetromino.</param>
        /// <param name="yPos">The highest Y coordinate of the matrix' position.</param>
        /// <param name="xPos">The lowest X coordinate of the matrix' position.</param>
        public Tetromino(Matrix matrix, int xPos, int yPos) {
            this.matrix = matrix;
            this.xPos = xPos;
            this.yPos = yPos;
        }

        public bool IsOnCoordinates(int xCoord, int yCoord) {
            int matrixHeight = matrix.Value.GetLength(0);
            int matrixWidth = matrix.Value.GetLength(1);

            for (int y = 0; y < matrixHeight; y++) {
                for (int x = 0; x < matrixWidth; x++) {
                    int currentCellXPos = xPos + x;
                    int currentCellYPos = yPos - (matrixHeight - 1 - y);
                    int tetroType = matrix.Value[y, x];

                    if (xCoord == currentCellXPos &&
                        yCoord == currentCellYPos &&
                        tetroType != 0) {
                        return true;
                    }
                }
            }

            return false;
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
            // Define the matrix coordinates
            new Matrix(new int[,] {
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
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
                { 0, 0, 0 },
            }),

            // T  ▄█▄
            new Matrix(new int[,] {
                { 0, 6, 0 },
                { 6, 6, 6 },
                { 0, 0, 0 },
            }),

            // Z  ▀█▄
            new Matrix(new int[,] {
                { 7, 7, 0 },
                { 0, 7, 7 },
                { 0, 0, 0 },
            })
        };

        /// <summary>
        ///  Chooses a random shape from the list of pre-defined Tetris shapes
        /// </summary>
        /// <returns> A random shape in the form of a Matrix </returns>
        public static Matrix RandomShape() {
            return shapes[new Random().Next(shapes.Count - 1)];
        }
    }
}