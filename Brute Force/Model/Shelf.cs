using System.Collections.Generic;

namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing a shelf.
    /// </summary>
    public class Shelf
    {
        #region Fields

        private bool available;
        private List<int> products; // list of products on the shelf
        private Coordinate baseCord; // base position of the shelf
        private Coordinate position; // position of the shelf
        private bool isOnRobot;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the availability of the shelf.
        /// </summary>
        public bool Available { get { return available; } set { available = value; } }

        /// <summary>
        /// Gets or sets the list of products on the shelf.
        /// </summary>
        public List<int> Products { get { return products; } set { products = value; } }

        /// <summary>
        /// Gets if the shelf is empty.
        /// </summary>
        public bool Empty { get { return Products.Count == 0; } }

        /// <summary>
        /// Gets or sets the position of the shelf.
        /// </summary>
        public Coordinate Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Gets the number of products on the shelf.
        /// </summary>
        public int NumOfProducts { get { return Products.Count; } }

        /// <summary>
        /// Gets or sets if the shelf is on a robot.
        /// </summary>
        public bool IsOnRobot { get { return isOnRobot; } set { isOnRobot = value; } }

        /// <summary>
        /// Gets or sets the base position of the shelf.
        /// </summary>
        public Coordinate BaseCord { get { return baseCord; } set { baseCord = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the shelf.
        /// </summary>
        /// <param name="x">The first coordinate of the shelf.</param>
        /// <param name="y">The second coordinate of the shelf.</param>
        public Shelf(int x, int y)
        {
            available = true;
            position = new Coordinate(x, y);
            baseCord = new Coordinate(x, y);
            products = new List<int>();
            isOnRobot = false;
        }

        /// <summary>
        /// Creating the shelf.
        /// </summary>
        public Shelf()
        {
            available = true;
            products = new List<int>();
            position = new Coordinate(-1, -1);
            baseCord = new Coordinate(-1, -1);
            isOnRobot = false;
        }

        #endregion
    }
}
