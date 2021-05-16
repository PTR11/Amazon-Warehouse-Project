namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing a station.
    /// </summary>
    public class Station
    {
        #region Fields

        private int id; // id of the station
        private Coordinate position; // position of the station

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the id of the station.
        /// </summary>
        public int Id { get { return id; } set { id = value; } }

        /// <summary>
        /// Gets or sets the position of the station.
        /// </summary>
        public Coordinate Position { get { return position; } set { position = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the station.
        /// </summary>
        /// <param name="x">The first coordinate of the station.</param>
        /// <param name="y">The second coordinate of the station.</param>
        /// <param name="stationId">The id of the station.</param>
        public Station(int x, int y, int stationId)
        {
            id = stationId;
            position = new Coordinate(x, y);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Takes product from shelf.
        /// </summary>
        /// <param name="shelf">The Shelf object from which the station takes the product.</param>
        public void TakePoduct(Shelf shelf)
        {
            shelf.Products.Remove(Id);
        }

        #endregion
    }
}
