using System.Collections.Generic;
using System.Linq;

namespace TetrisEngine {
    public static class Utils {
        /// <summary>
        /// A static class used to convert a Queue of matrices to a int[,].
        /// </summary>
        /// <param name="queue">The queue of matrices.</param>
        /// <returns>The queue as an int[,] object.</returns>
        public static int[,] QueueToIntArr(Queue<Matrix> queue) {
            int[,] newArr = new int[8, 4];
            List<Matrix> matrixList = queue.ToList();

            for (int i = 0; i < matrixList.Count; i++) {
                int shapeYStartPosition = i * 3; // Every shape will be positioned 3 cells lower than the previous.
                int[,] matrix = matrixList[i].Value;

                // Draw the queue cells in the queue grid
                for (int y = 0; y < matrix.GetLength(0); y++)
                for (int x = 0; x < matrix.GetLength(1); x++)
                    if (matrix[y, x] != 0)
                        newArr[y + shapeYStartPosition, x] = matrix[y, x];
            }

            return newArr;
        }
    }
}