namespace TetrisEngine {
    public class Scoring {
        public int Points { get; private set; }
        public int Lines { get; private set; }

        private int _pointsPerFall;
        private int _pointsPerSoft;
        private int _multiplierPerLand;
        private LineClearFormula _lineClearRewards;

        private delegate int LineClearFormula(int multiplierPerLine);

        /// <summary>
        /// Creates a new scoring instance using the normal game point system.
        /// Every time a piece falls one down, you get 0 points.
        /// Every time a piece is soft-dropped one down, you get 1 point.
        /// Every time a piece lands in it's final spot, you get 2 * the amount of cells the tetromino counts.
        /// Every time a single line is cleared, you get 50 points.
        /// Every time multiple lines are cleared, you get (lines * 2 - 1) * 50 points.
        /// </summary>
        public static Scoring NormalGame() {
            return new Scoring(
                pointsPerFall: 0,
                pointsPerSoft: 1,
                multiplierPerLand: 2,
                
                // With 50 points awarded per line (PPL):
                // If only one line was cleared, award 1 * PPL.
                // If there were multiple lines cleared at once, award lines_cleared * 2 - 1, times PPL.
                // This ensures a higher yield of points, the higher the amount of lines cleared at once.
                lineClearFormula: lines => lines > 1 ? (lines * 2 - 1) * 50 : 50);
        }

        /// <param name="pointsPerFall">The amount of points awarded for every automatic block drop.</param>
        /// <param name="pointsPerSoft">The amount of points awarded per soft move downwards.</param>
        /// <param name="multiplierPerLand">The amount of points awarded per amount of blocks a tetromino has</param>
        /// <param name="lineClearFormula">The delegate formula for awarding points per line cleared.</param>
        private Scoring(int pointsPerFall, int pointsPerSoft, int multiplierPerLand, LineClearFormula lineClearFormula) {
            Points = 0;
            _pointsPerFall = pointsPerFall;
            _pointsPerSoft = pointsPerSoft;
            _multiplierPerLand = multiplierPerLand;
            _lineClearRewards = lineClearFormula;
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

        /// <summary>
        /// The point handler for when 1 or more lines are cleared.
        /// </summary>
        /// <param name="amountOfLines">The amount of lines that were cleared.</param>
        public void LinesCleared(int amountOfLines) {
            Points += _lineClearRewards.Invoke(amountOfLines);
            Lines += amountOfLines;
        }
    }
}