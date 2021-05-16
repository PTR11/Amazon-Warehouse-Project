using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Brute_Force.Model;

namespace Brute_Force.ViewModel
{
    /// <summary>
    /// The datacontext class of view layer
    /// </summary>
    public class ViewModelClass : ViewModelBase
    {
        #region Fields
        private ModelClass _model; //For communication between model and viewModel
        public ObservableCollection<Field> Fields { get; set; } //The list of tables elements
        public List<Field> designatedShelf = new List<Field>(); //The list what "tables elements" are designated by the user
        public List<Shelf> designatedShelfList = new List<Shelf>(); //Contains shelves which is designated on the field
        private Int32 _lineN; //The line number of the table to the IntegerDropDown on the main view
        private Int32 _colN; //The column number of the table to the IntegerDropDown on the main view
        private Int32 _gridW;//The line number of the table
        private Int32 _gridH;//The column number of the table
        private Int32 _windowHeigth; //The actual height of the main window
        private Int32 _windowWidth; //The actual width of the main window
        private String _startStopContent; //The content of the start/stop button on the grid
        public static String isCheckedBy; //Store the last element what was right clicked by the user.
        public Int32 rightClickedButton; //Store index of the last element what was right clicked by the user.
        private Int32 _time; //The variable which helps to show actual step number on the screen.
        #endregion

        #region Getters & Setters
        public ModelClass Model { get => _model; set => _model = value; }
        public Int32 Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (_windowWidth != value)
                {
                    _windowWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 WindowHeight
        {
            get { return _windowHeigth; }
            set
            {
                if (_windowHeigth != value)
                {
                    _windowHeigth = value;
                    OnPropertyChanged();
                }
            }
        }
        public String StartStopContent
        {
            get { return _startStopContent; }
            set
            {
                if (_startStopContent != value)
                {
                    _startStopContent = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 LineN
        {
            get { return _lineN; }
            set
            {
                if (_lineN != value)
                {
                    _lineN = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 ColN
        {
            get { return _colN; }
            set
            {
                if (_colN != value)
                {
                    _colN = value;
                    OnPropertyChanged();
                }
            }
        }


        public Int32 GridW
        {
            get { return _gridW; }
            set
            {
                if (_gridW != value)
                {
                    _gridW = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 GridH
        {
            get { return _gridH; }
            set
            {
                if (_gridH != value)
                {
                    _gridH = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GameTime { get { return TimeSpan.FromSeconds(_model.GameTime).ToString("g"); } }

        #endregion

        #region Delegate Commands
        public DelegateCommand StartStopCommand { get; private set; }
        public DelegateCommand LoadSimulationCommand { get; private set; }
        public DelegateCommand SaveSimulationCommand { get; private set; }
        public DelegateCommand SaveTableCommand { get; private set; }
        public DelegateCommand ClickChargerCommand { get; private set; }
        public DelegateCommand ClickStationCommand { get; private set; }
        public DelegateCommand ClickRobotCommand { get; private set; }
        public DelegateCommand ClickShelfCommand { get; private set; }
        public DelegateCommand Delete { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        #endregion

        #region EventHandlers
        public static event EventHandler<ButtonType> RighClickEvent;
        public static event EventHandler TimerEvent;
        public static event EventHandler LoadSimulation;
        public static event EventHandler SaveSimulation;
        public static event EventHandler SaveTable;
        public static event EventHandler ExitProgram;
        public static event EventHandler EndSimulation;
        public static event EventHandler RefreshOrders;
        public static event EventHandler RefreshProductList;
        public static event EventHandler DeleteStation;
        #endregion

        #region Constuctor
        /// <summary>
        /// Main Constructor for the View Model. It calls initialize methods to set up everything in the view layer.
        /// </summary>
        /// <param name="model">For the communicaton, we give the model's reference to View Model which will be stored.</param>
        public ViewModelClass(ModelClass model)
        {
            _model = model;
            _model.TimeAdvanced += new EventHandler<int>(Model_TimeAdvanced);
            _model.EndOfSimulation += new EventHandler<EventArgs>(OnSimulationEnd);
            _model.OrdersChanged += new EventHandler<EventArgs>(OnRefreshOrders);
            _model.ProductListChanged += new EventHandler<EventArgs>(OnRefreshProductList);
            StartStopCommand = new DelegateCommand(param => OnStartStopSim());
            
            LoadSimulationCommand = new DelegateCommand(param => OnLoadGame());
            SaveSimulationCommand = new DelegateCommand(param => OnSaveGame());
            SaveTableCommand = new DelegateCommand(param => OnSaveTable());

            ClickChargerCommand = new DelegateCommand(param => OnChargerCLick());
            ClickRobotCommand = new DelegateCommand(param => OnRobotCLick());
            ClickStationCommand = new DelegateCommand(param => OnStationCLick());
            ClickShelfCommand = new DelegateCommand(param => OnShelfCLick());

            Delete = new DelegateCommand(param => DeleteObject());
            ExitCommand = new DelegateCommand(param => OnExitGame());

            Fields = new ObservableCollection<Field>();
            StartStopContent = "Start";
            GenerateWarehouse();
        }
        /// <summary>
        /// From the model depo's setup, the method shown initializes all of the elements of the grid.
        /// </summary>
        public void SetupGridLayout()
        {
            foreach (Field field in Fields)
            {
                int x = field.X;
                int y = field.Y;

                if (_model.Depo[x, y] == 'P')
                {
                    Shelf shelf = _model.ShelvesListProp.Find(r => r.Position.Equals(new Coordinate(x, y)));
                    if (shelf == null)
                    {
                        field.BaseImage = "/images/shelf2.png";
                        field.Image = "/images/shelf2.png";
                        field.Text = "";
                        field.Background = "White";
                        continue;
                    }
                    if (!shelf.IsOnRobot)
                    {
                        if (!shelf.Empty)
                        {
                            field.BaseImage = "/images/shelfwithHole.png";
                            field.Image = "/images/shelfwithHole.png";
                            field.Text = _model.ShelvesListProp.Find(r => r.Position.Equals(new Coordinate(x, y))).Products.Count.ToString();
                            field.Background = "White";
                        }
                        else
                        {
                            field.BaseImage = "/images/shelf2.png";
                            field.Image = "/images/shelf2.png";
                            field.Text = "";
                            field.Background = "White";
                        }
                    }
                    else
                    {
                        field.BaseImage = "/images/empty.png";
                        field.Image = "/images/empty.png";
                        field.Text = "";
                        field.Background = "White";
                    }
                }
                else if (_model.Depo[x, y] == 'R')
                {
                    Robot robo = _model.RobotsListProp.Find(r => r.Position.Equals(new Coordinate(x, y)));
                    if (robo != null && robo.ShelfOnTop == true)
                    {
                        field.BaseImage = "/images/rwsB.png";
                        field.Image = "/images/rwsB.png";
                    }
                    else
                    {
                        if (robo.Direction == Direction.Up)
                        {
                            field.BaseImage = "/images/directions/robotUp.png";
                            field.Image = "/images/directions/robotUp.png";
                        }
                        else if (robo.Direction == Direction.Right)
                        {
                            field.BaseImage = "/images/directions/robotRight.png";
                            field.Image = "/images/directions/robotRight.png";
                        }
                        else if (robo.Direction == Direction.Left)
                        {
                            field.BaseImage = "/images/directions/robotLeft.png";
                            field.Image = "/images/directions/robotLeft.png";
                        }
                        else if (robo.Direction == Direction.Down)
                        {
                            field.BaseImage = "/images/directions/robotDown.png";
                            field.Image = "/images/directions/robotDown.png";
                        }
                    }

                }
                else if (_model.Depo[x, y] == 'S')
                {
                    field.BaseImage = "/images/station.png";
                    field.Image = "/images/station.png";
                    field.Background = "White";
                }
                else if (_model.Depo[x, y] == 'D')
                {
                    field.Background = "White";
                    field.Image = "/images/charger.png";
                    field.BaseImage = "/images/charger.png";
                }
                else
                {
                    field.Background = "White";
                    field.BaseImage = "/images/empty.png";
                    field.Image = "/images/empty.png";
                    field.Text = "";
                }

            }
        }
        /// <summary>
        /// From the model depo's setup, this method initializes view's size.
        /// </summary>
        private void Initialize()
        {
            Fields.Clear();
            GridW = _model.Depo.Width;
            GridH = _model.Depo.Height;
            LineN = GridH;
            ColN = GridW;
            int size = _model.Depo.Width > 15 || _model.Depo.Height > 15 ? (_model.Depo.Width > 30 || _model.Depo.Height > 30 ? 20 : 30) : 40;
            WindowWidth = _model.Depo.Width;
            WindowHeight = _model.Depo.Height * size;
            isCheckedBy = "";
        }
        /// <summary>
        /// Method to set back field's elemnt properties.
        /// </summary>
        /// <param name="field">The chosen field element</param>
        private void SetBaseView(Field field)
        {
            field.Designation = "Black";
            field.BaseImage = "/images/empty.png";
            field.Image = "/images/empty.png";
            field.Text = "";
        }
        /// <summary>
        /// Initialize all element of grid's table (size, image, designation stb). 
        /// </summary>
        public void InitializeGridLayout()
        {
            int size = _model.Depo.Width > 15 || _model.Depo.Height > 15 ? (_model.Depo.Width > 30 || _model.Depo.Height > 30 ? 20 : 30) : 40;
            for (Int32 i = 0; i < _model.Depo.Height; i++)
            {
                for (Int32 j = 0; j < _model.Depo.Width; j++)
                {
                    Fields.Add(new Field
                    {
                        BaseImage = "/images/empty.png",
                        Background = "White",
                        Image = "/images/empty.png",
                        X = i,
                        Y = j,
                        Designation = "Black",
                        Width = size,
                        Height = size,
                        Number = i * _model.Depo.Width + j,
                        OnClick = new DelegateCommand(param => GridButtonClick(Convert.ToInt32(param))),
                        DesignationShelf = new DelegateCommand(param => OnDesignationShelf(Convert.ToInt32(param))),
                        OnRightClick = new DelegateCommand(param => RightClick(Convert.ToInt32(param)))
                    });
                }
            }
        }
        /// <summary>
        /// This method is called when the program gets a new input, and it has to "reload" the gid.
        /// </summary>
        /// <param name="model"></param>
        public void GenerateWarehouse(ModelClass model = null)
        {
            if(model != null)
            {
                this._model = model;
                _model.TimeAdvanced += new EventHandler<int>(Model_TimeAdvanced);
                _model.EndOfSimulation += new EventHandler<EventArgs>(OnSimulationEnd);
                _model.OrdersChanged += new EventHandler<EventArgs>(OnRefreshOrders);
                _model.ProductListChanged += new EventHandler<EventArgs>(OnRefreshProductList);
            }
            Initialize();
            InitializeGridLayout();
            SetupGridLayout();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Calculate the intersect coordinates of the showed routes.
        /// </summary>
        /// <param name="showedRobots">The owner robots of the showed routes.</param>
        /// <returns>Intersect of the routes.</returns>
        private List<Coordinate> Intersect(List<Robot> showedRobots)
        {
            List<Coordinate> result = new List<Coordinate>();
            foreach (Coordinate elso in showedRobots[0].Route)
            {
                foreach (Coordinate masodik in showedRobots[1].Route)
                {
                    if (elso.Equals(masodik))
                    {
                        result.Add(elso);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Calculate the what type of Button on the given index.
        /// </summary>
        /// <param name="v">Button index</param>
        /// <returns>Returns an enum, which represents buttontype.</returns>
        private ButtonType GetButtonType(int v)
        {
            ButtonType result = ButtonType.None;
            foreach (Field field in Fields)
            {
                if (field.Number == v)
                {
                    int x = field.X;
                    int y = field.Y;
                    switch (_model.Depo.GetValue(x, y))
                    {
                        case 'P':
                            result = ButtonType.Shelf;
                            break;
                        case 'R':
                            result = ButtonType.Robot;
                            break;
                        case 'D':
                            result = ButtonType.Charger;
                            break;
                        case 'S':
                            result = ButtonType.Station;
                            break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Calculate the given robot's route color. If it doesn't show route, it gives back None.
        /// </summary>
        /// <param name="robot">The robot, what we want to know, what colors of its route has</param>
        /// <returns>String witch represents a color</returns>
        private String GetRouteColor(Robot robot)
        {
            int showedRobotsCount = this.Model.RobotsListProp.Where(r => r.ShowingRoute).Count();
            String result = "";
            String color = "None";
            if (showedRobotsCount == 1)
            {
                Robot other = this.Model.RobotsListProp.Find(r => r.ShowingRoute && !r.Position.Equals(robot));
                color = other.ShowingRouteColor;
            }
            switch (color)
            {
                case "Red":
                    result = "Blue";
                    break;
                case "Blue":
                    result = "Red";
                    break;
                case "None":
                    result = "Blue";
                    break;
            }

            return result;
        }
        /// <summary>
        /// Helper method, which calculate that we can move a shelf in the given position
        /// </summary>
        /// <param name="dest">The destination where we want to put the shelf</param>
        /// <returns>Returns true if we can move the shelf, and false, if we want to put it on an overrindexed place, or the destination cordinate is not free.</returns>
        private bool CanMove(Coordinate dest)
        {
            try
            {
                if (_model.Depo.GetValue(dest.X, dest.Y) != '0')
                {
                    return false;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// The method which moves every designated shelf with the the absolute value of the distance. 
        /// </summary>
        /// <param name="id">The shelf what we want to move</param>
        /// <param name="dest">The destination where we want to put our shelf</param>
        private void MoveShelf(int id, Coordinate dest)
        {
            Shelf s = GetShelf(id);
            _model.Depo.SetValue(s.Position.X, s.Position.Y, "0");
            _model.OriginalDepo.SetValue(s.Position.X, s.Position.Y, "0");
            foreach (Field field in Fields)
            {
                if (field.X == s.Position.X && field.Y == s.Position.Y)
                {
                    field.Clear();
                }
            }
            _model.Depo.SetValue(dest.X, dest.Y, "P");
            _model.OriginalDepo.SetValue(dest.X, dest.Y, "P");
            s.Position = dest;
            foreach (Field field in Fields)
            {
                if (field.X == s.Position.X && field.Y == s.Position.Y)
                {
                    if (!s.Empty)
                    {
                        field.BaseImage = "/images/shelfwithHole.png";
                        field.Image = "/images/shelfwithHole.png";
                        field.Text = s.NumOfProducts.ToString();
                        field.Background = "White";
                    }
                    else
                    {
                        field.BaseImage = "/images/shelf2.png";
                        field.Image = "/images/shelf2.png";
                        field.Text = "";
                        field.Background = "White";
                    }

                }
            }
            designatedShelfList.Remove(s);
        }
        /// <summary>
        /// Put products to moved self
        /// </summary>
        public void ProductModifier()
        {
            foreach (Shelf modelShelf in _model.ShelvesListProp)
            {
                foreach (Shelf shelf in designatedShelfList)
                {
                    if (modelShelf.Position.Equals(shelf.Position))
                    {
                        modelShelf.Products.AddRange(shelf.Products);
                        _model.Depo.Products[shelf.Position.X, shelf.Position.Y] = shelf.Products;
                    }
                }
            }
            SetupGridLayout();
        }
        #endregion

        #region MainMethods
        /// <summary>
        /// Added the given id to model's order list.
        /// </summary>
        /// <param name="id">Product's id, what we want to order.</param>
        public void AddProductToModelsOrders(int id)
        {
            int count = Model.Depo.Orders.Select(o => o.ProductId == id).Count();
            Model.GetAllProducts();
            int number = Model.ProductsWithCount[id];
            Robot r = Model.RobotsListProp.Find(r => r.ShippingOrder == id);
            if(count < number && r == null)
                Model.Depo.Orders.Add(new Order { OrderTime = 0, ProductId = id });
        }
        /// <summary>
        /// Uplade the designated shelves with the given items.
        /// </summary>
        /// <param name="itemsList">These items what we want to put on the designated shelf.</param>
        public void AddProductToShelf(ObservableCollection<Item> itemsList)
        {
            foreach (Item item in itemsList)
            {
                foreach (Shelf shelf in designatedShelfList)
                {
                    if (item.Coord.ToString().Equals(shelf.Position.ToString()))
                    {
                        shelf.Products.Clear();
                        if (item.Products.Count != 0)
                        {
                            foreach (String product in item.Products)
                            {
                                shelf.Products.Add(Int32.Parse(product));
                                _model.Depo.Products[shelf.Position.X, shelf.Position.Y].Add(Int32.Parse(product));
                            }
                        }
                    }
                    
                    foreach (Field field in designatedShelf)
                    {
                        field.Designation = "Black";
                    }
                    
                }
            }
            designatedShelf.Clear();
            designatedShelfList = new List<Shelf>();
            SetupGridLayout();
        }
        /// <summary>
        /// Calls by right click event, and cause delete the clicked item.
        /// </summary>
        public void DeleteObject()
        {
            Field tmp = Fields[rightClickedButton];
            Debug.WriteLine(tmp.X + " " + tmp.Y);
            if (designatedShelf.Contains(tmp))
            {
                foreach (Field f in designatedShelf)
                {
                    _model.DeleteShelves(f.X, f.Y);
                    Shelf asd = designatedShelfList.Find(ds => ds.Position.Equals(new Coordinate(f.X, f.Y)));
                    designatedShelfList.Remove(asd);
                    SetBaseView(f);
                }
                designatedShelf.Clear();
            }
            else
            {
                switch (GetButtonType(rightClickedButton))
                {
                    case ButtonType.Station:
                        Station station = _model.StationsListProp.Find(stat => stat.Position.Equals(new Coordinate(tmp.X, tmp.Y)));
                        Shelf s = _model.ShelvesListProp.Find(shelf => shelf.Products.Contains(station.Id));
                        if (s != null)
                        {
                            MessageBoxResult result = MessageBox.Show("Olyan polcot akar törölni amihez polc van hozzárendelve, ezt csak úgy tudja, ha a polcot is törli. \nBiztos ebben?", "Brute Force", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                            if (result == MessageBoxResult.Yes)
                            {
                                List<Shelf> copyOfShelves = new List<Shelf>(_model.ShelvesListProp);
                                foreach (Shelf shelf in copyOfShelves)
                                {
                                    if (shelf.Products.Contains(station.Id))
                                    {
                                        _model.DeleteShelves(shelf.Position.X, shelf.Position.Y);
                                        Field temp = Fields[shelf.Position.X * _gridW + shelf.Position.Y];
                                        SetBaseView(temp);
                                    }
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        _model.DeleteStations(tmp.X, tmp.Y);
                        SetBaseView(tmp);
                        break;
                    case ButtonType.Robot:
                        _model.DeleteRobots(tmp.X, tmp.Y);
                        SetBaseView(tmp);
                        break;
                    case ButtonType.Charger:
                        _model.DeleteChargers(tmp.X, tmp.Y);
                        SetBaseView(tmp);
                        break;
                }
            }

        }
        /// <summary>
        /// Calculates is there a shelf in the current position of the clicked button.
        /// </summary>
        /// <param name="id">Chosen button id</param>
        /// <returns>A shelf if the given position is a shelf, and returns null if its not.</returns>
        public Shelf GetShelf(int id)
        {
            int x = id / _model.Depo.Width;
            int tmp = id - x * _model.Depo.Width;
            int y = tmp % _model.Depo.Height;

            return _model.ShelvesListProp.Find(r => r.Position.Equals(new Coordinate(x, y)));
        }
        /// <summary>
        /// Moves every designated shelf to the given coordinates.
        /// </summary>
        /// <param name="xDif">X value of the Coordinates</param>
        /// <param name="yDif">Y value of the Coordinates</param>
        /// <returns>Returns true if move was succeded, and false if it's not.</returns>
        public bool MoveDesignationShelf(int xDif, int yDif)
        {
            foreach (Field des in designatedShelf)
            {
                Coordinate dest = new Coordinate(des.X + xDif, des.Y + yDif);
                if (!CanMove(dest))
                {
                    return false;
                }
            }
            Field ds;
            while (designatedShelf.Count != 0)
            {
                ds = designatedShelf[0];
                int x = ds.X + xDif;
                int y = ds.Y + yDif;
                if (x < _model.Depo.Height && x >= 0 && y < _model.Depo.Width && y >= 0)
                {
                    MoveShelf(ds.Number, new Coordinate(x, y));
                    designatedShelf.Remove(ds);
                }
            }
            return true;
        }
        /// <summary>
        /// Delete every element of the depo's order list by calling CleanOrders of the Model.
        /// </summary>
        public void DeleteOrders()
        {
            Model.CleanOrders();
        }
        /// <summary>
        /// Calculates is there a robot in the current position of the clicked button.
        /// </summary>
        /// <param name="robotid">Chosen button id</param>
        /// <returns>A shelf if the given position is a robot, and returns null if its not.</returns>
        public Robot GetRobot(int robotid)
        {
            int x = robotid / _model.Depo.Width;
            int tmp = robotid - x * _model.Depo.Width;
            int y = tmp % _model.Depo.Height;

            return _model.RobotsListProp.Find(r => r.Position.Equals(new Coordinate(x, y)));
        }
        /// <summary>
        /// Display the route of the chosen robot.
        /// </summary>
        /// <param name="index">Position of the robot</param>
        /// <param name="visible">Indicates that we want to see, or we want to hide the route</param>
        public void ShowRoute(int index, bool visible)
        {
            Field act = Fields[index];
            int x = act.X;
            int y = act.Y;
            Robot robot = _model.RobotsListProp.Find(r => r.Position.X == x && r.Position.Y == y);
            if (robot.Route.Count == 0)
                _model.RobotRoute(robot);
            robot.ShowingRouteColor = GetRouteColor(robot);
            int showedRobotsCount = this.Model.RobotsListProp.Where(r => r.ShowingRoute).Count();
            foreach (Coordinate cord in robot.Route)
            {
                int xCord = cord.X;
                int yCord = cord.Y;
                Fields[xCord * _model.Depo.Width + yCord].Background = visible ? robot.ShowingRouteColor : "White";
            }
            if (visible)
                robot.ShowingRoute = true;
            else
            {
                if (showedRobotsCount == 2)
                {
                    Robot other = this.Model.RobotsListProp.Find(r => r.ShowingRoute && !r.Equals(robot));
                    foreach (Coordinate c in Intersect(this.Model.RobotsListProp.Where(r => r.ShowingRoute).ToList()))
                    {
                        Fields[c.X * _model.Depo.Width + c.Y].Background = other.ShowingRouteColor;
                    }
                }
                robot.ShowingRoute = false;
            }
        }
        #endregion

        #region DelegateCommands handlers
        /// <summary>
        /// Called by model. Increase time during the program runs, and changes the table grid elements.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void Model_TimeAdvanced(object sender, int e)
        {
            Time = _model.GameTime;
            SetupGridLayout();
        }
        /// <summary>
        /// Calls by click on elements of the editor list. Change the designated element to Charger.
        /// </summary>
        private void OnChargerCLick()
        {
            if (isCheckedBy == "Charger")
            {
                isCheckedBy = "";
            }
            else
            {
                isCheckedBy = "Charger";
            }

        }
        /// <summary>
        /// Change the designation property of the given shelf.
        /// </summary>
        /// <param name="index">The given shelf index's</param>
        private void OnDesignationShelf(int index)
        {
            Field act = Fields[index];
            if (GetShelf(index) != null)
            {
                if (act.Designation == "Black")
                {
                    act.Designation = "Red";
                    designatedShelf.Add(act);
                    designatedShelfList.Add(GetShelf(index));
                }
                else
                {
                    act.Designation = "Black";
                    designatedShelf.Remove(act);
                    designatedShelfList.Remove(GetShelf(index));
                }
            }
        }
        /// <summary>
        /// Change Start/Stop title on the window, and calls OnTimerStartStop event to start or stop the timer.
        /// </summary>
        private void OnStartStopSim()
        {
            if (StartStopContent == "Pause")
            {
                StartStopContent = "Start";
            }
            else
            {
                StartStopContent = "Pause";
            }
            OnTimerStartStop();
        }
        /// <summary>
        /// Calls by click on elements of the editor list. Change the designated element to Station.
        /// </summary>
        private void OnStationCLick()
        {
            if (isCheckedBy == "Station")
            {
                isCheckedBy = "";
            }
            else
            {
                isCheckedBy = "Station";
            }
        }
        /// <summary>
        /// Calls by click on elements of the editor list. Change the designated element to Robot.
        /// </summary>
        private void OnRobotCLick()
        {
            if (isCheckedBy == "Robot")
            {
                isCheckedBy = "";
            }
            else
            {
                isCheckedBy = "Robot";
            }
        }
        /// <summary>
        /// Calls by click on elements of the editor list. Change the designated element to Shelf.
        /// </summary>
        private void OnShelfCLick()
        {
            if (isCheckedBy == "Shelf")
            {

                isCheckedBy = "";
            }
            else
            {
                isCheckedBy = "Shelf";
            }
        }
        /// <summary>
        /// Add a new object to the table grid. The new object what is added, is the designated object from the editor.
        /// </summary>
        /// <param name="index">The current position's index</param>
        private void GridButtonClick(int index)
        {
            Field act = Fields[index];
            int x = act.X;
            int y = act.Y;
            if (isCheckedBy != "")
            {
                if (_model.Depo[x, y] == '0')
                {
                    switch (isCheckedBy)
                    {

                        case "Robot":
                            act.Image = "/images/directions/robotUp.png";
                            int id = _model.RobotsListProp.Count + 1;
                            _model.InitializeRobots(x, y, id);
                            _model.Depo.Robots++;
                            break;
                        case "Shelf":
                            act.Image = "/images/shelf2.png";
                            _model.InitializeShelves(x, y, true);
                            break;
                        case "Station":
                            act.Image = "/images/station.png";
                            int idS = _model.Depo.Stations + 1;
                            _model.InitializeStations(x, y, idS);
                            _model.Depo.Stations++;
                            break;
                        case "Charger":
                            act.Image = "/images/charger.png";
                            _model.InitializeChargers(x, y);
                            _model.Depo.Chargers++;
                            break;
                    }
                }
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Calls when user clicked on the tabla gird with right click. It gives back what is the type of the clicked object.
        /// </summary>
        /// <param name="v">The clicked object's index</param>
        private void RightClick(int v)
        {
            rightClickedButton = v;
            RighClickEvent?.Invoke(this, GetButtonType(v));
        }
        /// <summary>
        /// Start and stop the timer in the App class.
        /// </summary>
        private void OnTimerStartStop()
        {
            TimerEvent?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to open Open file dialog to load a simulation file.
        /// </summary>
        private void OnLoadGame()
        {
            LoadSimulation?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to open Open file dialog to save simulation details in the end of the running.
        /// </summary>
        private void OnSaveGame()
        {
            SaveSimulation?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to open Open file dialog to save a simulation file.
        /// </summary>
        private void OnSaveTable()
        {
            SaveTable?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to refresh orders list because something changed in the model.
        /// </summary>
        private void OnRefreshOrders(Object sender, EventArgs e)
        {
            RefreshOrders?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to refresh product list because something changed in the model.
        /// </summary>
        private void OnRefreshProductList(Object sender, EventArgs e)
        {
            RefreshProductList?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal to close the project.
        /// </summary>
        private void OnExitGame()
        {
            ExitProgram?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Send a signal about simulation is over and Stop the timer.
        /// </summary>
        private void OnSimulationEnd(Object sender, EventArgs e)
        {
            OnStartStopSim();
            EndSimulation?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}