using Brute_Force.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing the A* algorithm.
    /// </summary>
    public class AStar
    {
        #region Fields

        private Cell[,] array;
        private bool foundDest;

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the instance of the algorithm.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        public AStar(Depot depo)
        {
            array = new Cell[depo.Height, depo.Width];
            foundDest = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deleting existing array.
        /// </summary>
        public void DeleteArray()
        {
            array = new Cell[array.GetLength(0), array.GetLength(1)];
        }

        /// <summary>
        /// Traces the path from the source to destination.
        /// </summary>
        /// <param name="destination">The position of the destination.</param>
        /// <returns>The created path in a list of coordinates.</returns>
        public List<Coordinate> TracePath(Coordinate destination)
        {
            int row = destination.X;
            int col = destination.Y;
            List<Coordinate> result = new List<Coordinate>();
            Stack<Coordinate> path = new Stack<Coordinate>();

            while(!(array[row,col].parent.X == row && array[row, col].parent.Y == col))
            {
                path.Push(new Coordinate(row, col));
                int tempRow = array[row, col].parent.X;
                int tempCol = array[row, col].parent.Y;

                row = tempRow;
                col = tempCol;
            }
            path.Push(new Coordinate(row, col));

            while(path.Count != 0)
            {
                Coordinate p = path.Peek();
                path.Pop();
                result.Add(p);
            }
            result.RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Determines whether the specified position is valid.
        /// </summary>
        /// <param name="cord">The position to validate with the height and width.</param>
        /// <param name="width">The width of the depo.</param>
        /// <param name="height">The height of the depo.</param>
        /// <returns>True if the position is valid within the specified height and width, otherwise false.</returns>
        public bool IsValid(Coordinate cord, int height, int width)
        {
            return cord.X >= 0 && cord.X < height && cord.Y >= 0 && cord.Y < width;
        }

        /// <summary>
        /// Determines whether the specified position is unblocked.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="row">The first coordinate of the position.</param>
        /// <param name="col">The second coordinate of the position.</param>
        /// <param name="withoutShelf">The status whether the robot is with a shelf.</param>
        /// <returns>True if the position is unblocked, otherwise false.</returns>
        public bool IsUnBlocked(Depot depo, int row, int col, bool withoutShelf)
        {
            if (withoutShelf)
            {
                if (depo[row, col] != 'R' && depo[row, col] != 'S')
                    return true;
                else
                    return false;
            }
            else
            {
                if (depo[row, col] != 'R' && depo[row, col] != 'S' && depo[row, col] != 'P')
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified position is the destination.
        /// </summary>
        /// <param name="row">The first coordinate of the position.</param>
        /// <param name="col">The second coordinate of the position.</param>
        /// <param name="destination">The position of the destination.</param>
        /// <returns>True if the position is the destination, otherwise false.</returns>
        public bool IsDestination(int row, int col, Coordinate destination)
        {
            if (row == destination.X && col == destination.Y)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Calculates the h value from specified position to the destination.
        /// </summary>
        /// <param name="row">The first coordinate of the position.</param>
        /// <param name="col">The second coordinate of the position.</param>
        /// <param name="destination">The position of the destination.</param>
        /// <returns>The calculated h value.</returns>
        public double CalculateHValue(int row, int col, Coordinate destination)
        {
            return ((double)Math.Sqrt(
                    (row - destination.X) * (row - destination.X)
                    + (col - destination.Y) * (col - destination.Y)));
        }

        /// <summary>
        /// Searches free route from the start position to the destination.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="start">The position of the starting coordinate.</param>
        /// <param name="destination">The position of the destination.</param>
        /// <param name="withoutShelf">The status whether the robot is with a shelf.</param>
        /// <returns>True if a route is found to the destination, otherwise false.</returns>
        public bool Search(Depot depo, Coordinate start, Coordinate destination, bool withoutShelf)
        {
            if (!IsValid(start, depo.Height, depo.Width))
            {
                return false;
            }

            if (!IsValid(destination, depo.Height, depo.Width))
            {
                return false;
            }

            if (IsDestination(start.X, start.Y, destination))
            {
                return false;
            }

            bool[,] unavailable = new bool[depo.Height, depo.Width];

            for (int i = 0; i < depo.Height; i++)
            {
                for (int j = 0; j < depo.Width; j++)
                {
                    unavailable[i, j] = false;
                }
            }

            for (int i = 0; i < depo.Height; i++)
            {
                for (int j = 0; j < depo.Width; j++)
                {
                    this.array[i, j] = new Cell(new Coordinate(0, 0), 0, 0, 0);

                }
            }

            array[start.X, start.Y].parent = new Coordinate(start.X, start.Y);

            List<KeyValuePair<double, Coordinate>> open = new List<KeyValuePair<double, Coordinate>>();
            open.Add(new KeyValuePair<double, Coordinate>(0, start));


            while (open.Count != 0)
            {
                var first = open.First();
                open.Remove(first);

                int i = first.Value.X;
                int j = first.Value.Y;

                unavailable[i, j] = true;

                double gNew;
                double hNew;
                double fNew;

                // first direction

                if (IsValid(new Coordinate(i - 1, j), depo.Height, depo.Width))
                {
                    if (IsDestination(i - 1, j, destination))
                    {
                        array[i - 1, j].parent.X = i;
                        array[i - 1, j].parent.Y = j;
                        foundDest = true;
                        return true;
                    }
                    else if (unavailable[i - 1, j] == false && IsUnBlocked(depo, i - 1, j, withoutShelf))
                    {
                        gNew = array[i, j].g++;
                        hNew = CalculateHValue(i - 1, j, destination);
                        fNew = hNew + gNew;
                        if (array[i - 1, j].f == 0 || array[i - 1, j].f > fNew)
                        {
                            open.Add(new KeyValuePair<double, Coordinate>(fNew, new Coordinate(i - 1, j)));
                            array[i - 1, j].f = fNew;
                            array[i - 1, j].g = gNew;
                            array[i - 1, j].h = hNew;
                            array[i - 1, j].parent = new Coordinate(i, j);
                        }
                    }
                }

                // second direction

                if (IsValid(new Coordinate(i + 1, j), depo.Height, depo.Width))
                {
                    if (IsDestination(i + 1, j, destination))
                    {
                        array[i + 1, j].parent.X = i;
                        array[i + 1, j].parent.Y = j;
                        foundDest = true;
                        return true;
                    }
                    else if (unavailable[i + 1, j] == false && IsUnBlocked(depo, i + 1, j, withoutShelf))
                    {
                        gNew = array[i, j].g++;
                        hNew = CalculateHValue(i + 1, j, destination);
                        fNew = hNew + gNew;
                        if (array[i + 1, j].f == 0 || array[i + 1, j].f > fNew)
                        {
                            open.Add(new KeyValuePair<double, Coordinate>(fNew, new Coordinate(i + 1, j)));
                            array[i + 1, j].f = fNew;
                            array[i + 1, j].g = gNew;
                            array[i + 1, j].h = hNew;
                            array[i + 1, j].parent = new Coordinate(i, j);
                        }
                    }
                }

                // third direction

                if (IsValid(new Coordinate(i, j + 1), depo.Height, depo.Width))
                {
                    if (IsDestination(i, j + 1, destination))
                    {
                        array[i, j + 1].parent.X = i;
                        array[i, j + 1].parent.Y = j;
                        foundDest = true;
                        return true;
                    }
                    else if (unavailable[i, j + 1] == false && IsUnBlocked(depo, i, j + 1, withoutShelf))
                    {
                        gNew = array[i, j].g++;
                        hNew = CalculateHValue(i, j + 1, destination);
                        fNew = hNew + gNew;
                        if (array[i, j + 1].f == 0 || array[i, j + 1].f > fNew)
                        {
                            open.Add(new KeyValuePair<double, Coordinate>(fNew, new Coordinate(i, j + 1)));
                            array[i, j + 1].f = fNew;
                            array[i, j + 1].g = gNew;
                            array[i, j + 1].h = hNew;
                            array[i, j + 1].parent = new Coordinate(i, j);
                        }
                    }
                }

                // fourth direction

                if (IsValid(new Coordinate(i, j - 1), depo.Height, depo.Width))
                {
                    if (IsDestination(i, j - 1, destination))
                    {
                        array[i, j - 1].parent.X = i;
                        array[i, j - 1].parent.Y = j;
                        foundDest = true;
                        return true;
                    }
                    else if (unavailable[i, j - 1] == false && IsUnBlocked(depo, i, j - 1, withoutShelf))
                    {
                        gNew = array[i, j].g++;
                        hNew = CalculateHValue(i, j - 1, destination);
                        fNew = hNew + gNew;
                        if (array[i, j - 1].f == 0 || array[i, j - 1].f > fNew)
                        {
                            open.Add(new KeyValuePair<double, Coordinate>(fNew, new Coordinate(i, j - 1)));
                            array[i, j - 1].f = fNew;
                            array[i, j - 1].g = gNew;
                            array[i, j - 1].h = hNew;
                            array[i, j - 1].parent = new Coordinate(i, j);
                        }
                    }
                }
            }

            return foundDest;
        }

        #endregion
    }

    /// <summary>
    /// The class representing a cell.
    /// </summary>
    public class Cell
    {
        #region Fields

        public Coordinate parent; // parent coordinate
        public double f;
        public double g;
        public double h;

        #endregion

        #region Contructor

        /// <summary>
        /// Creating the cell.
        /// </summary>
        /// <param name="parent">The parent coordinate of the cell.</param>
        /// <param name="f">The sum of g and h.</param>
        /// <param name="g">The movement cost to move from the starting point.</param>
        /// <param name="h">The estimated movement cost to move from g to the final destination.</param>
        public Cell(Coordinate parent, double f, double g, double h)
        {
            this.parent = parent;
            this.f = f;
            this.g = g;
            this.h = h;
        }

        #endregion
    }
}
