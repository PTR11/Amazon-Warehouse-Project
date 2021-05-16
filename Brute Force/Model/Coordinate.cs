namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing a position.
    /// </summary>
    public class Coordinate
    {
        #region Fields

        private int x; // first coordinate
        private int y; // second coordinate

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the first coordinate of a position.
        /// </summary>
        public int X { get { return x; } set { x = value; } }

        /// <summary>
        /// Gets or sets the second coordinate of a position.
        /// </summary>
        public int Y { get { return y; } set { y = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the coordinate.
        /// </summary>
        /// <param name="x">The first coordinate of a position.</param>
        /// <param name="y">The second coordinate of a position.</param>
        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether two coordinate instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current coordinate.</param>
        /// <returns>True if the two coordinates are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Coordinate p = (Coordinate)obj;
                return (x == p.x) && (y == p.y);
            }
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current coordinate.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        /// <summary>
        /// Returns a string that represents the current coordinate.
        /// </summary>
        /// <returns>A string that represents the current coordinate.</returns>
        public override string ToString()
        {
            return "(" + x + " " + y + ")";
        }

        #endregion
    }
}
