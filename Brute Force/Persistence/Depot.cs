using Brute_Force.Model;
using System;
using System.Collections.Generic;

namespace Brute_Force.Persistence
{
    /// <summary>
    /// Class of the data storage.
    /// </summary>
    public class Depot
    {
        #region Fields

        private int height; // height of the depot
        private int width; // width of the depot
        private int robots; // number of robots
        private int chargers; // number of chargers
        private int stations; // number of stations
        private int maxEnergy; // the max energy of the robots
        private char[,] values; // depot data
        private List<int>[,] products; // list of products
        private List<Order> orders; //the list of orders what we get in runtime
        #endregion

        #region Properties

        public int Height { get { return height; } set { height = value; } }
        public int Width { get { return width; } set { width = value; } }
        public int Robots { get { return robots; } set { robots = value; } }
        public int Chargers { get { return chargers; } set { chargers = value; } }
        public int Stations { get { return stations; } set { stations = value; } }
        public int MaxEnergy { get { return maxEnergy; } set { maxEnergy = value; } }
        public char[,] Depo { get => values; set => values = value; }
        public char this[int x, int y] { get { return GetValue(x, y); } set { values[x, y] = value; } }
        public List<int>[,] Products { get { return products; } set { products = value; } }
        public List<Order> Orders { get => orders; set => orders = value; }

        #endregion

        #region Constuctor

        /// <summary>
        /// Creating the depot.
        /// </summary>
        /// <param name="_height">Height of the depot.</param>
        /// <param name="_width">Width of the depot.</param>
        public Depot(int _height, int _width)
        {
            if (_height < 0) throw new ArgumentOutOfRangeException("The depot height is less than 0.", "Height");
            if (_width < 0) throw new ArgumentOutOfRangeException("The depot width is less than 0.", "Width");

            height = _height;
            width = _width;
            maxEnergy = 100;

            values = new char[_height, _width];
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    values[i, j] = '0';
                }
            }

            products = new List<int>[_height, _width];
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    products[i, j] = new List<int>();
                }
            }
            orders = new List<Order>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Getting the value of the specific coordinates.
        /// </summary>
        /// <param name="x">The first coordinate.</param>
        /// <param name="y">The second coordinate.</param>
        /// <returns>The character from the coordinates.</returns>
        public char GetValue(int x, int y)
        {
            if (x < 0 || x >= height)
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range." + x);
            if (y < 0 || y >= width)
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range." + y);

            return values[x, y];
        }

        /// <summary>
        /// Setting the value of the specific coordinates.
        /// </summary>
        /// <param name="x">The first coordinate.</param>
        /// <param name="y">The second coordinate.</param>
        /// <param name="value">The value to be set.</param>
        public void SetValue(int x, int y, string value)
        {
            if (x < 0 || x >= values.GetLength(0))
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= values.GetLength(1))
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");

            values[x, y] = value.ToCharArray()[0];
        }

        /// <summary>
        /// Getting the products from the specific coordinates.
        /// </summary>
        /// <param name="x">The first coordinate.</param>
        /// <param name="y">The second coordinate.</param>
        /// <returns>The list of products from the coordinates.</returns>
        public List<int> GetProductsvalue(int x, int y)
        {
            if (x < 0 || x >= width)
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range." + x);
            if (y < 0 || y >= height)
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range." + y);

            return products[x, y];
        }

        /// <summary>
        /// Adding a value to the specific coordinates.
        /// </summary>
        /// <param name="x">The first coordinate.</param>
        /// <param name="y">The second coordinate.</param>
        /// <param name="value">The product to be added.</param>
        public void SetProductsValue(int x, int y, int value)
        {
            if (x < 0 || x >= values.GetLength(0))
                throw new ArgumentOutOfRangeException("x", "The X coordinate is out of range.");
            if (y < 0 || y >= values.GetLength(1))
                throw new ArgumentOutOfRangeException("y", "The Y coordinate is out of range.");

            products[x, y].Add(value);
        }

        public void AddOrdersToOrderList(int orderId, int orderTime)
        {
            orders.Add(new Order()
            {
                OrderTime = orderTime,
                ProductId = orderId
            });
        }

        #endregion
    }
}
