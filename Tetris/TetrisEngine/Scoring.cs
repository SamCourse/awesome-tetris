namespace TetrisEngine {
    public class Scoring {
        public int Points;
        private int _pointsPerFall;
        private int _pointsPerSoft;
        private int _multiplierPerLand;
        
        /// <summary>
        /// Creates a new scoring instance using the normal game point system.
        /// Every time a piece falls one down, you get 0 points.
        /// Every time a piece is soft-dropped one down, you get 1 point.
        /// Every time a piece lands in it's final spot, you get 2 * the amount of cells the tetromino counts.
        /// </summary>
        public static Scoring NormalGame() {
            return new Scoring(0, 1, 2);
        }
        
        /// <param name="pointsPerSoft">The amount of points you get per soft move downwards.</param>
        /// <param name="multiplierPerLand">The amount of points you get per amount of blocks a tetromino has</param>
        private Scoring(int pointsPerFall, int pointsPerSoft, int multiplierPerLand) {
            Points = 0;
            _pointsPerFall = pointsPerFall;
            _pointsPerSoft = pointsPerSoft;
            _multiplierPerLand = multiplierPerLand;
        }

        /// <summary>
        /// The point handler for when a piece falls down a cell.
        /// </summary>
        public void Fall() {
            Points += _pointsPerFall;
        }

        /// <summary>
        /// The point handler for when a soft drop is done.
        /// </summary>
        public void SoftDrop() {
            Points += _pointsPerSoft;
        }

        /// <summary>
        /// The point handler for when a piece lands on it's final spot.
        /// </summary>
        /// <param name="cells">The amount of cells the tetromino has.</param>
        public void Land(int cells) {
            Points += _multiplierPerLand * cells;
        }
        
        
    }
}