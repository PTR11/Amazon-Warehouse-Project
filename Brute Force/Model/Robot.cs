using Brute_Force.Persistence;
using System;
using System.Collections.Generic;

namespace Brute_Force.Model
{
    /// <summary>
    /// The enum representing the direction of a robot.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Right,
        Left,
        None
    }

    /// <summary>
    /// The enum representing the destination of a robot.
    /// </summary>
    public enum DestPoint
    {
        ToPickUpShelf,
        ToStations,
        ToCharger,
        ToPutDownShelf,
        ToPickUpEmptyShelf
    }

    /// <summary>
    /// The class representing a robot.
    /// </summary>
    public class Robot
    {
        #region Fields

        private int energy; // energy of the robot
        private int usedEnergy; // used energy of the robot
        private int id; // id of the robot
        private int onCharger; // time spent on charger
        private bool shelfOnTop;
        private bool showingRoute;
        private bool waiting;
        private String showingRouteColor; // color of the showed route
        private Coordinate shelfCord = new Coordinate(-1, -1);
        private Coordinate position; // position of the robot
        private List<Coordinate> route; // route of the robot
        private Shelf deliverdShelf; // shelf assigned to the robot
        private Shelf emptyShelf; // empty shelf assigned to the robot
        private Direction direction; // direction of the robot
        private Charger charger; // charger assigned to the robot
        private List<Station> destinationStations; // stations assigned to the robot
        private DestPoint actualDestPoint; // shelf assigned to the robot
        private DestPoint prevDestPoint; // shelf assigned to the robot
        private bool toChargerWithShelf;
        private int shippingOrder; // order of the shippings

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the energy of the robot.
        /// </summary>
        public int Energy { get { return energy; } set { energy = value; } }

        /// <summary>
        /// Gets or sets the used energy of the robot.
        /// </summary>
        public int UsedEnergy { get { return usedEnergy; } set { usedEnergy = value; } }

        /// <summary>
        /// Gets or sets the id of the robot.
        /// </summary>
        public int Id { get { return id; } set { id = value; } }

        /// <summary>
        /// Gets or sets the time the robot on the charger has spent.
        /// </summary>
        public int OnCharger { get { return onCharger; } set { onCharger = value; } }

        /// <summary>
        /// Gets or sets the order of the shipping of the robot.
        /// </summary>
        public int ShippingOrder { get { return shippingOrder; } set { shippingOrder = value; } }

        /// <summary>
        /// Gets or sets whether the robot has a shelf on it or not.
        /// </summary>
        public bool ShelfOnTop { get { return shelfOnTop; } set { shelfOnTop = value; } }

        /// <summary>
        /// Gets or sets whether the route of the robot is showing or not.
        /// </summary>
        public bool ShowingRoute { get { return showingRoute; } set { showingRoute = value; } }

        /// <summary>
        /// Gets or sets whether the robot is waiting or not.
        /// </summary>
        public bool Waiting { get { return waiting; } set { waiting = value; } }

        /// <summary>
        /// Gets or sets whether the robot is going to a charger with a shelf or not.
        /// </summary>
        public bool ToChargerWithShelf { get { return toChargerWithShelf; } set { toChargerWithShelf = value; } }

        /// <summary>
        /// Gets or sets the color of the showed route.
        /// </summary>
        public String ShowingRouteColor { get { return showingRouteColor; } set { showingRouteColor = value; } }

        /// <summary>
        /// Gets or sets the position of the robot.
        /// </summary>
        public Coordinate Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Gets or sets the route of the robot.
        /// </summary>
        public List<Coordinate> Route { get { return route; } set { route = value; } }

        /// <summary>
        /// Gets or sets the shelf assigned to the robot.
        /// </summary>
        public Shelf DeliverdShelf { get { return deliverdShelf; } set { deliverdShelf = value; } }

        /// <summary>
        /// Gets or sets the empty shelf assigned to the robot.
        /// </summary>
        public Shelf EmptyShelf { get { return emptyShelf; } set { emptyShelf = value; } }

        /// <summary>
        /// Gets or sets the direction of the robot.
        /// </summary>
        public Direction Direction { get { return direction; } set { direction = value; } }

        /// <summary>
        /// Gets or sets the charger assigned to the robot.
        /// </summary>
        public Charger Charger { get { return charger; } set { charger = value; } }

        /// <summary>
        /// Gets or sets the list of stations assigned to the robot.
        /// </summary>
        public List<Station> DestinationStations { get { return destinationStations; } set { destinationStations = value; } }

        /// <summary>
        /// Gets or sets the current destination of the robot.
        /// </summary>
        public DestPoint ActualDestPoint { get { return actualDestPoint; } set { actualDestPoint = value; } }

        /// <summary>
        /// Gets or sets the previous destination of the robot.
        /// </summary>
        public DestPoint PrevDestPoint { get { return prevDestPoint; } set { prevDestPoint = value; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the robot.
        /// </summary>
        /// <param name="x">The first coordinate of the robot.</param>
        /// <param name="y">The second coordinate of the robot.</param>
        /// <param name="robotId">The id of the robot.</param>
        public Robot(int x, int y, int robotId)
        {
            id = robotId;
            energy = 100;
            usedEnergy = 0;
            shippingOrder = 0;
            shelfOnTop = false;
            showingRoute = false;
            toChargerWithShelf = false;
            showingRouteColor = "None";
            emptyShelf = new Shelf();
            deliverdShelf = new Shelf();
            route = new List<Coordinate>();
            position = new Coordinate(x, y);
            direction = Direction.Up;
            actualDestPoint = DestPoint.ToPickUpShelf;
        }

        /// <summary>
        /// Creating the robot.
        /// </summary>
        public Robot() : this(0, 0, 0) { }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether two robot instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current robot.</param>
        /// <returns>True if the two robots are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Robot r = (Robot)obj;
                return id == r.id;
            }
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current robot.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Sets the energy of the robot to max.
        /// </summary>
        /// <param name="max">The max value of the energy.</param>
        public void Charge(int max)
        {
            energy = max;
            this.charger.Robot = null;
            this.charger = null;
        }

        /// <summary>
        /// Moves the robot to the next position.
        /// </summary>
        public void Move()
        {
            if (energy <= 0)
            {
                if (charger != null && Position.Equals(charger.Position))
                    Charge(100);
                else
                    return;
            }

            if (Rotate())
            {
                --energy;
                usedEnergy++;
                return;
            }
            else
            {
                if (route.Count != 0)
                {
                    --energy;
                    usedEnergy++;
                    position = route[0];
                    route.RemoveAt(0);
                    if (this.position.Equals(deliverdShelf.Position) )
                    {
                        if (!waiting && route.Count == 0) 
                            shelfOnTop = !shelfOnTop;

                        if (deliverdShelf != null && route.Count == 0)
                            deliverdShelf.IsOnRobot = !deliverdShelf.IsOnRobot;
                    }
                }
            }
        }

        public Coordinate NextStep()
        {
            if (NextDirection() != Direction.None)
            {
                return route[0];
            }
            else
            {
                if (route.Count > 0)
                {
                    return route[0];
                }
                else
                {
                    return Position;
                }
            }
        }

        /// <summary>
        /// Gets the next direction of the robot.
        /// </summary>
        /// <returns>The next direction of the robot.</returns>
        private Direction NextDirection()
        {
            Direction tmpDir;
            if (this.route.Count > 0)
            {
                if (this.route[0].X == Position.X)
                {
                    if (this.route[0].Y == Position.Y + 1)
                    {
                        tmpDir = Direction.Right;
                        return tmpDir;
                    }
                    else if (this.route[0].Y == Position.Y - 1)
                    {
                        tmpDir = Direction.Left;
                        return tmpDir;
                    }
                }
                else if (route[0].Y == Position.Y)
                {
                    if (route[0].X == Position.X + 1)
                    {
                        tmpDir = Direction.Down;
                        return tmpDir;
                    }
                    else if (route[0].X == Position.X - 1)
                    {
                        tmpDir = Direction.Up;
                        return tmpDir;
                    }
                }
            }

            tmpDir = Direction.None;
            return tmpDir;
        }

        private Direction GetRightDirectionForRobot(Direction dir)
        {
            if ((dir == Direction.Up && this.direction == Direction.Down) || (dir == Direction.Down && this.direction == Direction.Up))
            {
                return Direction.Left;
            }
            else if ((dir == Direction.Left && this.direction == Direction.Right) || (dir == Direction.Up && this.direction == Direction.Down))
            {
                return Direction.Up;
            }
            return dir;
        }
        /// <summary>
        /// Decides if the robot needs to rotate.
        /// </summary>
        /// <returns>True if the robot needs to rotate, otherwise false.</returns>
        private bool Rotate()
        {
            Direction tmpDir = NextDirection();
            if (tmpDir == Direction.None || tmpDir == this.direction)
            {
                return false;
            }
            else
            {
                this.direction = GetRightDirectionForRobot(tmpDir);
                return true;
            }
        }

        /// <summary>
        /// Finds a path to a station.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="shelfPos">The position of the shelf where the search is run from.</param>
        /// <param name="shelf">A shelf object.</param>
        /// <param name="stations">The list of stations where the path leads.</param>
        /// <returns>A list of coordinates which lead to a station</returns>
        public List<Coordinate> ToStation(Depot depo, Coordinate shelfPos, Shelf shelf, List<Station> stations)
        {
            AStar a = new AStar(depo);
            List<Coordinate> _route = new List<Coordinate>();

            DestinationStations = stations;
            DeliverdShelf = shelf;
            bool found = a.Search(depo, shelfPos, stations[0].Position, false);
            if (!found) return _route;

            _route.AddRange(a.TracePath(stations[0].Position));
            for (int i = 1; i < stations.Count; i++)
            {
                a.DeleteArray();
                found = a.Search(depo, stations[i - 1].Position, stations[i].Position, false);
                if (!found) return _route;
                _route.AddRange(a.TracePath(stations[i].Position));
            }

            return _route;
        }

        /// <summary>
        /// Finds a path to a shelf.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="from">The starting coordinate.</param>
        /// <param name="to">The destination coordinate.</param>
        /// <param name="withoutShelf">Whether the robot is without shelf or not.</param>
        /// <returns>A list of coordinates which lead to a shelf</returns>
        public List<Coordinate> ToShelf(Depot depo, Coordinate from, Coordinate to, bool withoutShelf)
        {
            AStar a = new AStar(depo);
            List<Coordinate> _route = new List<Coordinate>();

            bool found = a.Search(depo, from, to, withoutShelf);
            if (!found) return _route;

            _route.AddRange(a.TracePath(to));
            return _route;
        }

        /// <summary>
        /// Finds a path to a charger.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="charger">The charger object where the path leads.</param>
        /// <param name="withoutShelf">Whether the robot is without shelf or not.</param>
        /// <returns>A list of coordinates which lead to a shelf</returns>
        public List<Coordinate> ToCharger(Depot depo, Charger charger, bool withoutShelf)
        {
            AStar a = new AStar(depo);
            List<Coordinate> _route = new List<Coordinate>();

            bool found = a.Search(depo, Position, charger.Position, withoutShelf);
            if (!found) return _route;
            _route.AddRange(a.TracePath(charger.Position));
            charger.Robot = this;

            return _route;
        }

        /// <summary>
        /// Plans the route of the robot.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="_route">The route which is to be planned.</param>
        /// <param name="charger">The closest charger to the robot.</param>
        /// <param name="replanned">Whether the route is replanned or not.</param>
        public void RoutePlanner(Depot depo, List<Coordinate> _route, Charger charger, bool replanned)
        {
            Route.AddRange(_route);

            Robot tmp = new Robot();
            tmp.Position = this.position;
            tmp.Direction = this.direction;
            tmp.Route.AddRange(Route);
            int actRoad = tmp.RouteEnergyCalculator();

            tmp = new Robot();
            tmp.Position = Position;
            if (!Position.Equals(charger.Position))
            {
                tmp.Route.AddRange(ToCharger(depo, charger, !ShelfOnTop));

                int goToChargerRoad = tmp.RouteEnergyCalculator();
                int robotEnergyMinusActRoad = Energy - actRoad;
                if (robotEnergyMinusActRoad < goToChargerRoad + 4)
                {
                    RouteToCharger(depo, charger,replanned);
                    return;
                }
                charger.Robot = null;
            }
        }

        /// <summary>
        /// Plans the route of the robot to the charger.
        /// </summary>
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="charger">The destination charger.</param>
        /// <param name="replanned">Whether the route is replanned or not.</param>
        public void RouteToCharger(Depot depo, Charger charger, bool replanned)
        {
            Route.Clear();
            if (ShelfOnTop)
            {
                if (actualDestPoint == DestPoint.ToStations)
                {
                    PutDownShelf();
                    Route.AddRange(ToCharger(depo, charger, true));
                }
                else 
                {
                    List<Coordinate> toCharger = ToCharger(depo, charger, false);
                    Route.AddRange(toCharger);
                    toChargerWithShelf = true;
                    EmptyShelf.Position = toCharger.Count==1 ? this.Position : toCharger[^2];
                }
            }
            else
            {
                PutDownShelf();
                Route.AddRange(ToCharger(depo, charger, true));
            }

            Charger = charger;
            charger.Robot = this;
            SetToCharger(replanned);
        }

        /// <summary>
        /// Puts down the shelf from the robot.
        /// </summary>
        private void PutDownShelf()
        {
            ShelfOnTop = false;
            DeliverdShelf.Available = true;
            DeliverdShelf.IsOnRobot = false;
            DeliverdShelf = new Shelf();
        }

        /// <summary>
        /// Calculates the amount of energy the route requires.
        /// </summary>
        /// <returns>The amount of energy the route requires.</returns>
        public int RouteEnergyCalculator()
        {
            int length = this.route.Count;
            if(length != 0) { 
                Coordinate lastPos = this.route[^1];
                int c = 0;
                while(this.position != lastPos)
                {
                    c++;
                    this.Move();
                }
            }

            return 100 - this.energy;
        }

        /// <summary>
        /// Makes the robot wait.
        /// </summary>
        /// <param name="time">The amount of the the robot needs to wait.</param>
        /// <param name="copy">The params witch tells we need a copy of the route</param>
        public void Wait(int time, bool copy = false)
        {
            List<Coordinate> copyBeforeClear = new List<Coordinate>(route);
            waiting = true;
            route.Clear();
            List<Coordinate> stay = new List<Coordinate>();
            for (int i = 0; i < time; i++)
            {
                this.energy++;
                stay.Add(new Coordinate(position.X, position.Y));
            }
            route.AddRange(stay);
            if (copy)
                route.AddRange(copyBeforeClear);
            
        }

        /// <summary>
        /// Replans the route of the robot.
        /// </summary>        
        /// <param name="depo">The depo within the search is run.</param>
        /// <param name="charger">The closest charger to the robot.</param>
        /// <param name="waiting">Whether the robot needs to wait or not.</param>
        /// <param name="time">The time the robot needs to wait.</param>
        public void Replanning(Depot depo, Charger charger, bool waiting = false, int time = 0)
        {
            List<Coordinate> coords = new List<Coordinate>();
            this.route.Clear();

            if (waiting)
                Wait(time);

            switch (this.prevDestPoint)
            {
                case DestPoint.ToPickUpShelf:
                    coords = ToShelf(depo, this.position, this.deliverdShelf.Position, true);
                    break;
                case DestPoint.ToPickUpEmptyShelf:
                    charger.Robot = null;
                    coords = ToShelf(depo, this.position, this.emptyShelf.Position, true);
                    break;
                case DestPoint.ToStations:
                    coords = ToStation(depo, this.position, this.DeliverdShelf, DestinationStations);
                    break;
                case DestPoint.ToPutDownShelf:
                    coords = ToShelf(depo, this.Position, this.DeliverdShelf.BaseCord, false);
                    break;
                case DestPoint.ToCharger:
                    coords = ToCharger(depo, charger, !this.ShelfOnTop);
                    break;
                default:
                    break;
            }

            if (coords.Count == 0)
                this.ActualDestPoint = this.PrevDestPoint;

            if(prevDestPoint != DestPoint.ToCharger)
                RoutePlanner(depo, coords, charger, true);
        }

        /// <summary>
        /// Sets the robots destination to charger.
        /// </summary>        
        /// <param name="replanned">Whether the route is replanned or not.</param>
        private void SetToCharger(bool replanned = false)
        {
            if (!(ShelfOnTop && replanned)) prevDestPoint = actualDestPoint;
            this.actualDestPoint = DestPoint.ToCharger;
        }

        /// <summary>
        /// Sets the robots destination to the next destination.
        /// </summary>    
        public void NextDestPoint()
        {
            DestPoint tmp = this.actualDestPoint;
            if (toChargerWithShelf && this.actualDestPoint == DestPoint.ToCharger)
            {
                this.actualDestPoint = DestPoint.ToPickUpEmptyShelf;
                return;
            }

            switch (this.actualDestPoint)
            {
                case DestPoint.ToPickUpShelf:
                    this.actualDestPoint = DestPoint.ToStations;
                    break;
                case DestPoint.ToPickUpEmptyShelf:
                    this.actualDestPoint = this.prevDestPoint;
                    break;
                case DestPoint.ToStations:
                    this.actualDestPoint = DestPoint.ToPutDownShelf;
                    break;
                case DestPoint.ToPutDownShelf:
                    this.actualDestPoint = DestPoint.ToPickUpShelf;
                    break;
                case DestPoint.ToCharger:
                    this.actualDestPoint = DestPoint.ToPickUpShelf;
                    break;
                default:
                    break;
            }

            this.prevDestPoint = tmp;
        }

        #endregion
    }
}
