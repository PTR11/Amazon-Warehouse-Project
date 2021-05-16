using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Brute_Force.Persistence;

namespace Brute_Force.Model
{
    /// <summary>
    /// Class of the model.
    /// </summary>
    public class ModelClass
    {
        #region Events

        public event EventHandler<Depot> DepotInitialized;
        public event EventHandler<Depot> DepotLoadedFromFile;
        public event EventHandler<int> TimeAdvanced;
        public event EventHandler<EventArgs> EndOfSimulation;
        public event EventHandler<EventArgs> OrdersChanged;
        public event EventHandler<EventArgs> ProductListChanged;
        public event EventHandler<EventArgs> OrdersArrive;
        #endregion

        #region Fields

        private Depot depot; //sotre the current data of the warehouse
        private Depot originalDepot; //sotre the original data of the warehouse
        private IDataAccess dataAccess; //writes/reads to/from a file
        private int gameTime; //elapsed time unit
        
        private Dictionary<int, int> products = new Dictionary<int, int>(); //number of products by type
        private List<Charger> ChargersList; //the list of chargers in the warehouse
        private List<Robot> RobotsList; //the list of robots in the warehouse
        private List<Shelf> ShelvesList; //the list of shelves in the warehouse
        private List<Station> StationsList; //the list of stations in the warehouse



        #endregion

        #region Properties

        public List<Charger> ChargersListProp { get { return ChargersList; } }
        public List<Robot> RobotsListProp { get { return RobotsList; } }
        public List<Shelf> ShelvesListProp { get { return ShelvesList; } }
        public List<Station> StationsListProp { get { return StationsList; } }
        public Depot Depo { get => depot; set => depot = value; }
        public Depot OriginalDepo { get => originalDepot; set => originalDepot = value; }
        public int GameTime { get { return gameTime; } }
        public Dictionary<int, int> ProductsWithCount { get { return products; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creating the model. Set the starting values of the variables.
        /// </summary>
        /// <param name="height">The height of the depot.</param>
        /// <param name="width">The width of the depot.</param>
        /// <param name="access">An instance of data access class</param>
        public ModelClass(int height, int width, IDataAccess access)
        {
            dataAccess = access;
            gameTime = 0;
            depot = new Depot(height, width);
            originalDepot = new Depot(height, width);
            ChargersList = new List<Charger>();
            RobotsList = new List<Robot>();
            ShelvesList = new List<Shelf>();
            StationsList = new List<Station>();
            OnDepotInitialized();
        }

        #endregion

        #region ProductMethod

        /// <summary>
        /// Investigate every shelf to count the products by type. The result is in the 'products' dictionary.
        /// </summary>
        /// <returns></returns>
        public List<int> GetAllProducts()
        {
            products.Clear();
            foreach(Shelf shelf in ShelvesListProp)
            {
                foreach(int item in shelf.Products)
                {
                    if (products.ContainsKey(item))
                    {
                        int value = products[item];
                        products[item] = value + 1;
                    }
                    else
                    {
                        products.Add(item, 1);
                    }
                }
            }

            foreach(int item in products.Keys)
            {
                Debug.WriteLine(item +": "+products[item]);
            }
            List<int> result = products.Keys.ToList();
            result.Sort();
            return result ;
        }



        #endregion

        #region MoveAtTick
        /// <summary>
        /// Plan the route path to the next destination.
        /// It will automaticly find the closest charger and the closest shelf if needed.
        /// If the robot doesn't have enough energy it will go to the charger.
        /// This is a void function. The result is in the robot's route.
        /// </summary>
        /// <param name="robot">The robot that we want to plan its route to the destination</param>
        /// <param name="show">The param witch helps to plan a route and delete it, if we want to show the route on the view</param>
        public void RobotRoute(Robot robot, bool show = false)
        {
            List<Coordinate> planned = new List<Coordinate>();
            Shelf closestShelf;
            if (robot.Charger != null)
            {
                robot.Charger.Robot = null;
            }
            switch (robot.ActualDestPoint)
            {
                case DestPoint.ToPickUpShelf:
                    closestShelf = ClosestShelf(robot);
                    if (closestShelf == null)
                    {
                        return;
                    }
                    else
                    {
                        if (robot.ShippingOrder != 0)
                        {
                            if(Depo.Orders.Count > 0)
                            {
                                Depo.Orders.Remove(Depo.Orders[0]);
                                OnOrdersChanged();
                            }                        
                        }
                        planned = robot.ToShelf(Depo, robot.Position, closestShelf.Position, true);
                        robot.DeliverdShelf = closestShelf;
                        robot.DeliverdShelf.Available = false;
                    }
                    break;
                case DestPoint.ToPickUpEmptyShelf:
                    if (robot.Position.Equals(robot.EmptyShelf.Position))
                    {
                        robot.NextDestPoint();
                        return;
                    }
                    planned = robot.ToShelf(Depo,robot.Position,robot.EmptyShelf.Position, true);
                    break;
                case DestPoint.ToStations:

                    List<Station> stationList = new List<Station>();
                    foreach (int product in robot.DeliverdShelf.Products)
                    {
                        Station destination = StationsList.Find(i => i.Id == product);
                        stationList.Add(destination);
                    }
                    planned = robot.ToStation(Depo, robot.Position, robot.DeliverdShelf, stationList);
                    break;
                case DestPoint.ToPutDownShelf:
                    if(!robot.Position.Equals(robot.DeliverdShelf.BaseCord))
                        planned = robot.ToShelf(Depo, robot.Position, robot.DeliverdShelf.BaseCord, false);
                    break;
                default:
                    break;
            }

            Charger closestCharger = new Charger();
            if (robot.Charger == null) closestCharger = ClosestCharger(robot);
            else closestCharger = robot.Charger;

            robot.RoutePlanner(Depo, planned, closestCharger, false);
            
            if (robot.Route.Count == 0 && robot.ActualDestPoint != DestPoint.ToCharger)
            {
                robot.Wait(3);
                return;
            }
            robot.NextDestPoint();
        }

        /// <summary>
        /// This is a void function.
        /// This function preforms operations per unit time with robots.
        /// If the robot has not route, we call the planner function.
        /// We examine whether the robot has a free route, if not, we add the robot the conflict list.
        /// The other case, the robot take on step, and it does its job (pick up or put down the shelf, charge or rotate).
        /// At the end of the examine we iterate through the conflict list, and replanned the robot's route or wait.
        /// At the end of the function, check the end of simulation condition, if yes send a signal to the view model.
        /// </summary>
        public void MoveAtTick()
        {
            RefreshOrdersByTime();
            if (gameTime == 0) OnProductListChange();
            List<int> robotsWithConflict = new List<int>();
            foreach (Robot robot in RobotsList)
            {
                if (robot.Route.Count == 0)
                {
                    RobotRoute(robot, false);
                    continue;
                }

                if (!robot.Position.Equals(robot.Route[0]))
                {
                    robot.Waiting = false;
                }

                if (IsRouteFree(robot))
                {
                    if (originalDepot[robot.Position.X, robot.Position.Y] != 'R')
                    {
                        if (originalDepot[robot.Position.X, robot.Position.Y] == 'P' && (ShelvesList.Find(p => p.Position.Equals(robot.Position) && p.IsOnRobot) != null))
                        {
                            depot[robot.Position.X, robot.Position.Y] = '0';
                        }
                        else
                        {
                            if ((robot.PrevDestPoint == DestPoint.ToStations || robot.PrevDestPoint == DestPoint.ToCharger) && robot.Position.Equals(robot.DeliverdShelf.BaseCord))
                            {
                                depot[robot.Position.X, robot.Position.Y] = '0';
                            }
                            else {
                                depot[robot.Position.X, robot.Position.Y] = originalDepot[robot.Position.X, robot.Position.Y];
                            }
                        }
                    }
                    else if (depot[robot.Position.X, robot.Position.Y] != '0')
                    {
                        depot[robot.Position.X, robot.Position.Y] = '0';
                    }
                    if (robot.DeliverdShelf != null && robot.Position.Equals(robot.DeliverdShelf.BaseCord) && robot.ShelfOnTop)
                    {
                        depot[robot.DeliverdShelf.Position.X, robot.DeliverdShelf.Position.Y] = '0';
                    }

                    if (robot.Charger != null && robot.Position.Equals(robot.Charger.Position) && robot.OnCharger < 5)
                    {
                        robot.OnCharger++;
                        depot[robot.Position.X, robot.Position.Y] = 'R';
                        if (robot.OnCharger == 5)
                        {
                            robot.Charge(Depo.MaxEnergy);
                        }
                    }
                    else
                    {
                        if (robot.ToChargerWithShelf && robot.Route.Count == 1)
                        {
                            if (depot[robot.Position.X, robot.Position.Y] != 'D')
                            {
                                originalDepot[robot.Position.X, robot.Position.Y] = 'P';
                                depot[robot.Position.X, robot.Position.Y] = 'P';
                                Depo.Products[robot.Position.X, robot.Position.Y] = robot.DeliverdShelf.Products;
                            }
                            robot.ShelfOnTop = false;
                        }
                        else if (robot.ToChargerWithShelf && robot.Position.Equals(robot.EmptyShelf.Position))
                        {
                            originalDepot[robot.Position.X, robot.Position.Y] = '0';
                            depot[robot.Position.X, robot.Position.Y] = '0';
                            robot.ShelfOnTop = true;
                            robot.ToChargerWithShelf = false;
                        }

                        robot.Move();
                        depot[robot.Position.X, robot.Position.Y] = 'R';
                        OnTimeAdvanced();
                        if(robot.DeliverdShelf.NumOfProducts != 0)
                        {
                            Station tmp = IsStation(robot.Position);
                            if (tmp != null && robot.DeliverdShelf.Products.Contains(tmp.Id))
                            {
                                if (robot.ShippingOrder == tmp.Id)
                                {
                                    Debug.WriteLine("Megrendelés leszállítva("+robot.Id+"):" + robot.DeliverdShelf.BaseCord.X + "," + robot.DeliverdShelf.BaseCord.Y);
                                    OrdersArrive?.Invoke(this, EventArgs.Empty);
                                    robot.ShippingOrder = 0;
                                }
                                else
                                {
                                    foreach (int productItem in robot.DeliverdShelf.Products)
                                    {
                                        Order o = Depo.Orders.Find(or => or.ProductId == productItem);
                                        if (productItem == tmp.Id && o != null)
                                        {
                                            Depo.Orders.Remove(o);

                                            Debug.WriteLine("Megrendelés leszállítva:" + robot.DeliverdShelf.BaseCord.X + "," + robot.DeliverdShelf.BaseCord.Y);
                                            OrdersArrive?.Invoke(this, EventArgs.Empty);
                                            OnOrdersChanged();
                                        }
                                    }
                                }
                                
                                tmp.TakePoduct(robot.DeliverdShelf);
                                OnProductListChange();
                                robot.DestinationStations.Remove(tmp);
                            }
                        }
                        

                        robot.OnCharger = 0;
                    }
                }
                else
                {
                    robotsWithConflict.Add(robot.Id);
                }

            }
            this.gameTime++;
            if (robotsWithConflict.Count != 0)
            {
                
                int counter = 0;
                foreach (Robot item in RobotsList)
                {
                    if (item.Id == robotsWithConflict[counter] && !IsRouteFree(item))
                    {
                        if (item.ToChargerWithShelf && !IsRouteFree(item))
                        {
                            item.Wait(item.Id,true);
                            continue;
                        }
                        if (CanNotMove(item))
                        {
                            item.Route.Clear();
                            item.ActualDestPoint = item.PrevDestPoint;
                            item.Wait(1);
                            continue;
                        }
                        if (item.Charger != null && item.Position.Equals(item.Charger.Position))
                        {
                            continue;
                        }
                        
                        if (item.Charger != null)
                        {
                            item.Charger.Robot = null;
                        }

                        if (Depo[item.Route[0].X, item.Route[0].Y] == 'R')
                        {
                            if (item.NextStep().Equals(item.DeliverdShelf.BaseCord) && !item.ShelfOnTop)
                            {
                                Shelf closest = ClosestShelf(item);
                                item.DeliverdShelf.Available = true;
                                item.DeliverdShelf = closest;
                            }else if(item.NextStep().Equals(item.DeliverdShelf.BaseCord) && !item.ShelfOnTop)
                            {
                                item.Wait(item.Id + 2);
                                continue;
                            }
                            if (item.PrevDestPoint==DestPoint.ToCharger)
                            {
                                item.Wait(item.Id+2);
                                continue;
                            }item.Replanning(Depo, ClosestCharger(item), true, item.Id*2);
                        }
                        else
                        {
                            item.Replanning(Depo, ClosestCharger(item));
                        }

                        counter++;
                        if (counter == robotsWithConflict.Count)
                        {
                            break;
                        }
                    }
                }
            }

            if (SimulationEnd())
            {
                OnEndOfSimulation();
            }
        }

        /// <summary>
        /// The return value is true or false;
        /// The function return FALSE, if the robot CAN move.
        /// 
        /// False, if the robot has empty field, or shelf if it is not carrying a shelf, next to it (up, dwon, left, right).
        /// True, every other case.
        /// 
        /// The function return TRUE, if the robot CAN NOT move.
        /// </summary>
        /// <param name="robot">The robot, we want to check it can not move.</param>
        /// <returns></returns>
        private bool CanNotMove(Robot robot)
        {
            for (int x = Math.Max(0, robot.Position.X - 1); x <= Math.Min(robot.Position.X + 1, Depo.Height-1); x++)
            {
                for (int y = Math.Max(0, robot.Position.Y - 1); y <= Math.Min(robot.Position.Y + 1, Depo.Width-1); y++)
                {
                    if (!(Math.Abs(x - robot.Position.X) == 1 && Math.Abs(y - robot.Position.Y) == 1)) {
                        if (x != robot.Position.X || y != robot.Position.Y)
                        {
                            if (Depo[x, y] == '0' || (Depo[x,y]=='P' && !robot.ShelfOnTop))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// The retur value is true or false;
        /// The route is free(true), if on the robot's route next element position is not a robot or not a shelf if the robot is carrying a shelf.
        /// </summary>
        /// <param name="robot">The robot, we want to check has free route.</param>
        /// <returns></returns>
        private bool IsRouteFree(Robot robot)
        {
            if (robot.NextStep().Equals(robot.Position))
            {
                return true;
            }

            if (Depo[robot.NextStep().X, robot.NextStep().Y] == 'R' || (Depo[robot.NextStep().X, robot.NextStep().Y] == 'P' && robot.ShelfOnTop))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Return with a charger from the charger list, which is closest to the given robot and empty.
        /// </summary>
        /// <param name="robot">The robot to which we want the closest charger</param>
        /// <returns></returns>
        private Charger ClosestCharger(Robot robot)
        {
            double min = Double.MaxValue;
            Charger closestCharger = new Charger();

            foreach (Charger charger in ChargersList)
            {
                if (charger.Robot == robot || charger.Robot == null)
                {

                    double temp = (double)Math.Sqrt(
                                Math.Pow((charger.Position.X - robot.Position.X), 2)
                                + Math.Pow((charger.Position.Y - robot.Position.Y), 2));
                    if (temp < min)
                    {
                        closestCharger = charger;
                        min = temp;
                    }
                }
            }

            if (min == Double.MaxValue)
                return null;
            else
                return closestCharger;

        }
        /// <summary>
        /// If we have any orders, it will return the first of them, else return null;
        /// </summary>
        /// <returns></returns>
        private Order GetActualOrder()
        {
            if (Depo.Orders.Count != 0 &&Depo.Orders[0].OrderTime == 0)
            {
                return Depo.Orders[0];
            }
            else return null;
        }
        /// <summary>
        /// If we reach the order's time, it will put on the top of orders list.
        /// </summary>
        private void RefreshOrdersByTime()
        {
            foreach (Order order in Depo.Orders)
            {
                if (order.OrderTime <= gameTime)
                {
                    order.OrderTime = -1;
                }
            }

            Depo.Orders.Sort(delegate (Order x, Order y)
            {
                return x.OrderTime.CompareTo(y.OrderTime);
            });

            foreach (Order order in Depo.Orders)
            {
                if (order.OrderTime == -1)
                {
                    order.OrderTime = 0;
                }
            }

            OnOrdersChanged();
        }
        /// <summary>
        /// Return the closest shelf with products, which is available (Not assigned to any robot). This shelf is waiting to deliver its products.
        /// If there is no shelf with that conditions, null value will return.
        /// </summary>
        /// <param name="robot">The robot to which we want the closest available shelf</param>
        /// <returns></returns>
        public Shelf ClosestShelf(Robot robot)
        {
            double min = Double.MaxValue;
            Shelf closestShelf = new Shelf();
            Order order = GetActualOrder();
            foreach (Shelf shelf in ShelvesList)
            {
                bool expression = shelf.Available && shelf.Products.Count != 0;
                if (robot.ShippingOrder == 0 && order != null)
                {
                    expression = shelf.Available && shelf.Products.Count != 0 && shelf.Products.Contains(order.ProductId);
                    robot.ShippingOrder = order.ProductId;
                }
                else if(robot.ShippingOrder != 0)
                    expression = shelf.Available && shelf.Products.Count != 0 && shelf.Products.Contains(robot.ShippingOrder);

                if (expression)
                {
                    double temp = (double)Math.Sqrt(
                                Math.Pow((shelf.Position.X - robot.Position.X), 2)
                                + Math.Pow((shelf.Position.Y - robot.Position.Y), 2));
                    if (temp < min)
                    {
                        closestShelf = shelf;
                        min = temp;
                    }
                }
            }

            if (min == Double.MaxValue)
                return null;
            else
                return closestShelf;
        }
        /// <summary>
        /// Return true if there is a station on the coordinate.
        /// Return false if there is any other element on the coordinate.
        /// </summary>
        /// <param name="cord">The coordinate, where we find a station</param>
        /// <returns></returns>
        public Station IsStation(Coordinate cord)
        {
            return StationsList.FirstOrDefault(s => s.Position.Equals(cord));
        }
        /// <summary>
        /// This function check the end conditions.
        /// If there is not any shelf with products and any robot is not carrying shelf and no one of the robots is going to charger with shelf.
        /// In that case end of simulation and true value will return.
        /// Every other case false will return.
        /// </summary>
        /// <returns></returns>
        private bool SimulationEnd()
        {
            Shelf s = this.ShelvesListProp.Find(sf => sf.Products.Count != 0);
            Robot r = this.RobotsListProp.Find(rb => rb.ShelfOnTop == true);
            Robot d = this.RobotsListProp.Find(rb => rb.ToChargerWithShelf == true);

            if (s != null || r != null || d != null) return false;

            return true;
        }
        /// <summary>
        /// Delete all of orders from the ordres list.
        /// </summary>
        public void CleanOrders()
        {
            Depo.Orders.Clear();
            OnOrdersChanged();
        }

        #endregion

        #region Loading and Saving
        /// <summary>
        /// Read data from the source file and start the setup of the depot.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        public async Task LoadDepot(String path, String extension)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");
            if (extension.Equals(".matrix")){
                depot = await dataAccess.LoadAsyncMatrix(path);
                originalDepot = await dataAccess.LoadAsyncMatrix(path);
            }
            else
            {
                depot = await dataAccess.LoadAsync(path);
                originalDepot = await dataAccess.LoadAsync(path);
            }
            
            gameTime = 0;
            MainInitializer();
            OnDepotLoadedFromFile();
            GetAllProducts();
        }
        /// <summary>
        /// Write statistics of simulation to the target file.
        /// Statistics: simulation time, sum of used energy, use energy of each robots
        /// </summary>
        /// <param name="path">The target file path.</param>
        /// <returns></returns>
        public async Task SaveStatistics(String path)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await dataAccess.SaveStatsAsync(path, gameTime, RobotsList);
        }
        /// <summary>
        /// Write all of data of depot to a file, which will abel to read data and start simulation with that.
        /// </summary>
        /// <param name="path">The target file path.</param>
        /// <returns></returns>
        public async Task SaveDepot(String path)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            await dataAccess.SaveDepotAsync(path, depot);
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Start elements initialization.
        /// First of all clear all lists (robots, stations, shelves, chargers).
        /// Then iterate through every field and call the appropriate initializer for each element.
        /// </summary>
        public void MainInitializer()
        {
            RobotsList.Clear();
            StationsList.Clear();
            ShelvesList.Clear();
            ChargersList.Clear();

            int robotId = 1;
            int stationId = 1;

            for (int i = 0; i < depot.Height; i++)
            {
                for (int j = 0; j < depot.Width; j++)
                {
                    switch (depot.Depo[i, j])
                    {
                        case 'P':
                            InitializeShelves(i, j, false);
                            break;

                        case 'S':
                            InitializeStations(i, j, stationId);
                            stationId++;
                            break;

                        case 'R':
                            InitializeRobots(i, j, robotId);
                            robotId++;
                            break;

                        case 'D':
                            InitializeChargers(i, j);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Add a robot to the robots list with the given position and ID, and set the depot value 'R' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="robotId">Uniq id</param>
        public void InitializeRobots(int x, int y, int robotId)
        {
            RobotsList.Add(new Robot(x, y, robotId));
            if (Depo.GetValue(x, y) == '0')
            {
                Depo.SetValue(x, y, "R");
            }
            if (originalDepot.GetValue(x, y) == '0')
            {
                originalDepot.SetValue(x, y, "R");
            }
        }
        /// <summary>
        /// Delete robot from the robots list where the robot position equals the given coordinate, and set the depot value '0' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void DeleteRobots(int x, int y)
        {
            Robot tmp = RobotsList.Find(r => r.Position.Equals(new Coordinate(x, y)));
            if (tmp != null)
            {
                if(tmp.ShelfOnTop && tmp.DeliverdShelf.Position.Equals(new Shelf().Position))
                {
                    ShelvesList.Remove(tmp.DeliverdShelf);
                    Depo.Products[tmp.DeliverdShelf.Position.X, tmp.DeliverdShelf.Position.Y].Clear();
                }
                RobotsList.Remove(tmp);
                Depo.SetValue(x, y, "0");
                Depo.Robots--;
            }
        }
        /// <summary>
        /// Add a station to the statins list with the given position and ID, and set the depot value 'S' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="stationId">Uniq id</param>
        public void InitializeStations(int x, int y, int stationId)
        {
            StationsList.Add(new Station(x, y, stationId));
            if (Depo.GetValue(x, y) == '0')
            {
                Depo.SetValue(x, y, "S");
            }
            if (originalDepot.GetValue(x, y) == '0')
            {
                originalDepot.SetValue(x, y, "S");
            }
        }
        /// <summary>
        /// Delete station from the stations list where the station position equals the given coordinate, and set the depot value '0' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void DeleteStations(int x, int y)
        {
            Station tmp = StationsList.Find(r => r.Position.Equals(new Coordinate(x, y)));
            if (tmp != null)
            {
                StationsList.Remove(tmp);
                Depo.SetValue(x, y, "0");
                Depo.Stations--;
            }
        }
        /// <summary>
        /// Add a shelf to the shelves list with the given position, and set the depot value 'P' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="inEditor">Put from editor</param>
        public void InitializeShelves(int x, int y, bool inEditor)
        {
            ShelvesList.Add(new Shelf(x, y));
            if (Depo.GetValue(x, y) == '0')
            {
                Depo.SetValue(x, y, "P");
            }

            if (originalDepot.GetValue(x, y) == '0')
            {
                originalDepot.SetValue(x, y, "P");
            }
            if (!inEditor)
            {
                if(depot.Products[x,y].Count != 0)
                {
                    if (depot.Products[x, y][0] == 0)
                    {
                        ShelvesList[^1].Products = new List<int>();
                    }
                    else
                    {
                        ShelvesList[^1].Products = depot.Products[x, y];
                    }
                }
            }
        }
        /// <summary>
        /// Delete shelf from the shelves list where the shelf position equals the given coordinate, and set the depot value '0' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void DeleteShelves(int x, int y)
        {
            Shelf tmp = ShelvesList.Find(r => r.Position.Equals(new Coordinate(x, y)));
            if (tmp != null)
            {
                ShelvesList.Remove(tmp);
                Depo.SetValue(x, y, "0");
                Depo.Products[x, y].Clear();
            }
        }
        /// <summary>
        /// Add a charger to the chargers list with the given position, and set the depot value 'D' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void InitializeChargers(int x, int y)
        {
            ChargersList.Add(new Charger(x, y));
            if (Depo.GetValue(x, y) == '0')
            {
                Depo.SetValue(x, y, "D");
            }
            if (originalDepot.GetValue(x, y) == '0')
            {
                originalDepot.SetValue(x, y, "D");
            }
        }
        /// <summary>
        /// Delete charger from the chargers list where the charger position equals the given coordinate, and set the depot value '0' at the given coordinate.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void DeleteChargers(int x, int y)
        {
            Charger tmp = ChargersList.Find(r => r.Position.Equals(new Coordinate(x, y)));
            if (tmp != null)
            {
                ChargersList.Remove(tmp);
                Depo.SetValue(x, y, "0");
                Depo.Chargers--;
            }
        }

        #endregion

        #region EventsFunction
        /// <summary>
        /// Send a signal when depot initialized.
        /// </summary>
        private void OnDepotInitialized()
        {
            DepotInitialized?.Invoke(this, depot);
        }
        /// <summary>
        /// Send a signal when depot loaded from file.
        /// </summary>
        private void OnDepotLoadedFromFile()
        {
            DepotLoadedFromFile?.Invoke(this, depot);
        }
        /// <summary>
        /// Send a signal when orders list changed.
        /// </summary>
        public void OnOrdersChanged()
        {
            OrdersChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal when time increased.
        /// </summary>
        public void OnTimeAdvanced()
        {
            TimeAdvanced?.Invoke(this, gameTime);
        }
        /// <summary>
        /// Send a signal when end of simulation conditions completed.
        /// </summary>
        public void OnEndOfSimulation()
        {
            EndOfSimulation?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal when products list changed.
        /// </summary>
        public void OnProductListChange()
        {
            GetAllProducts();
            ProductListChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
