namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing a charger.
    /// </summary>
    public class Charger
    {
        #region Fields

        private Coordinate position; // position of the charger
        private Robot robot; // robot assigned to charger

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the position of the charger.
        /// </summary>
        public Coordinate Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Gets or sets the robot assigned to the charger.
        /// </summary>
        public Robot Robot { get { return robot; } set { robot = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the charger.
        /// </summary>
        /// <param name="x">The first coordinate of the charger.</param>
        /// <param name="y">The second coordinate of the charger.</param>
        public Charger(int x, int y)
        {
            position = new Coordinate(x, y);
            robot = null;
        }

        /// <summary>
        /// Creating the charger.
        /// </summary>
        public Charger()
        {
            position = new Coordinate(0, 0);
            robot = null;
        }

        #endregion
    }
}
