using Brute_Force.Model;
using Brute_Force.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Brute_Force.View
{
    /// <summary>
    /// Class for display products name's
    /// </summary>
    public class ProductsString
    {
        public String NameOfProduct { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public event EventHandler<Coordinate> ResizeEvent;
        public Button designatedBtn = new Button();
        public Button dragButton;
        private bool mouseMove;
        private DispatcherTimer timer;
        public ObservableCollection<Order> items = new ObservableCollection<Order>();
        public ObservableCollection<ProductsString> products = new ObservableCollection<ProductsString>();

        /// <summary>
        /// Main window's constructor
        /// </summary>
        /// <param name="t">Timer witch given in the App.xaml</param>
        /// <param name="vm">Viewmodel, to setup the communication betwen view and its model</param>
        public MainWindow(DispatcherTimer t, ViewModelClass vm)
        {
            this.DataContext = vm;
            timer = t;
            InitializeComponent();
            RefreshProductsAndOrders(vm, true);
            
        }

        #region Events
        /// <summary>
        /// Makes on order of selected
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event of the selection changed</param>
        private void OrderClickedProduct(object sender, SelectionChangedEventArgs e)
        {
            ClickedOnProduct();
        }
        /// <summary>
        /// Designated buttons on the grid
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void ChangeBorderBrush(object sender, RoutedEventArgs e)
        {
            string button = (sender as Button).Name.ToString();
            switch (button)
            {
                case "Charger":
                    ChangeColor(ViewModelClass.isCheckedBy, Charger);
                    break;
                case "Robot":
                    ChangeColor(ViewModelClass.isCheckedBy, Robot);
                    break;
                case "Station":
                    ChangeColor(ViewModelClass.isCheckedBy, Station);
                    break;
                case "Shelf":
                    ChangeColor(ViewModelClass.isCheckedBy, Shelf);
                    break;
            }
        }
        /// <summary>
        /// Delete the clicked object
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event of the selection changed</param>
        private void Delete(object sender, RoutedEventArgs e)
        {
            ViewModelClass vm = (ViewModelClass)this.DataContext;
            vm.DeleteObject();
        }
        /// <summary>
        /// Show and hide the chosen robot's route
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void ShowRoute(object sender, RoutedEventArgs e)
        {

            ViewModelClass viewModel = (ViewModelClass)this.DataContext;
            Robot robot = viewModel.GetRobot(viewModel.rightClickedButton);
            if (robot.ShowingRoute)
            {
                viewModel.ShowRoute(viewModel.rightClickedButton, false);
            }
            else
            {
                viewModel.ShowRoute(viewModel.rightClickedButton, true);
            }

        }
        /// <summary>
        /// When clicked on robot datas menu, it opens a new window, to see every important data.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void ShowRobotData(object sender, RoutedEventArgs e)
        {
            ViewModelClass viewModel = (ViewModelClass)this.DataContext;
            RobotData robotDataWindow = new RobotData(viewModel);
            robotDataWindow.Show();
            robotDataWindow.Closed += new EventHandler(CloseWindow);
            this.IsEnabled = false;

        }
        /// <summary>
        /// Opens a new window to add product to shelves
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void ShelfProperties(object sender, RoutedEventArgs e)
        {
            ViewModelClass viewModel = (ViewModelClass)this.DataContext;
            ShelfProduct ShelfPropertiesWindow = new ShelfProduct(viewModel);

            ShelfPropertiesWindow.Show();
            ShelfPropertiesWindow.Closed += new EventHandler(CloseWindow);
            this.IsEnabled = false;
        }
        /// <summary>
        /// Drag&Drop drag function, which check what we can drag.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void DragButton(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            if (!mouseMove)
            {
                mouseMove = true;
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    Field context;
                    if (btn.DataContext is Field field)
                    {
                        context = field.Copy();
                        ViewModelClass viewModel = (ViewModelClass)this.DataContext;
                        if (viewModel.GetShelf(context.Number) != null)
                        {
                            dragButton = btn;
                            DragDrop.DoDragDrop(btn, context, DragDropEffects.Copy);
                            e.Handled = true;
                        }
                        else
                        {
                            mouseMove = false;
                        }
                    }
                }
                mouseMove = false;
            }
        }
        /// <summary>
        /// Checks, if we can put the shelves there, and check this move is correct
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void DropButton(object sender, DragEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn != dragButton)
            {
                if (btn.Content == null)
                {
                    if (e.Data.GetData(typeof(Field)) is Field)
                    {
                        Field dropBtn;
                        Field dragBtn;


                        ViewModelClass viewModel = (ViewModelClass)this.DataContext;
                        if (btn.DataContext is Field field)
                        {
                            dropBtn = field.Copy();
                            if (dragButton.DataContext is Field f)
                            {
                                dragBtn = f.Copy();
                                int xDif = dropBtn.X - dragBtn.X;
                                int yDif = dropBtn.Y - dragBtn.Y;
                                Debug.WriteLine("x: " + xDif + " y:" + yDif);
                                if (dragBtn.Designation.Equals("Red"))
                                {
                                    if (viewModel.designatedShelf.Count != 0)
                                    {
                                        if (!viewModel.MoveDesignationShelf(xDif, yDif))
                                        {
                                            MessageBox.Show("A polcok mozgatása nem sikerült", "Brute Force", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                        else
                                        {
                                            if (dragButton.DataContext is Field dragbtn)
                                            {
                                                dragbtn.Designation = "Black";
                                                dragbtn.Background = "White";
                                                dragbtn.Text = "";
                                                dragbtn.BaseImage = "/images/empty.png";
                                                dragbtn.Image = "/images/empty.png";
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Nem működik");
                }
            }
        }
        /// <summary>
        /// Close the window.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void CloseWindow(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }
        /// <summary>
        /// Refresh the window with the given size.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void Refresh(object sender, RoutedEventArgs e)
        {
            ResizeEvent?.Invoke(this, new Coordinate((int)rowsNumber.Value, (int)colsNumber.Value));
        }
        /// <summary>
        /// Slowe the time.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void Slower(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            var interval = timer.Interval;
            timer.Interval = interval + TimeSpan.FromSeconds(0.05);
            timer.Start();
        }
        /// <summary>
        /// Faster the time.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void Faster(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            var interval = timer.Interval;
            if (interval - TimeSpan.FromSeconds(0.05) > TimeSpan.Zero)
            {
                timer.Interval = interval - TimeSpan.FromSeconds(0.05);
            }
            timer.Start();
        }
        /// <summary>
        /// Add the chosen items once more to orders.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void DoubleClickOnListView(object sender, MouseButtonEventArgs e)
        {
            ClickedOnProduct();
        }
        /// <summary>
        /// Deleting every orders.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The event</param>
        private void Delete_orders(object sender, RoutedEventArgs e)
        {
            ViewModelClass viewModel = (ViewModelClass)this.DataContext;
            viewModel.DeleteOrders();

        }
        #endregion

        #region Methods
        /// <summary>
        /// Orders the chosen product
        /// </summary>
        private void ClickedOnProduct()
        {
            if (products.Count != 0)
            {
                ProductsString product = (ProductsString)orderlist.SelectedItem;
                Int32 productId = Convert.ToInt32(product.NameOfProduct.Split(".")[0]);
                ViewModelClass vm = (ViewModelClass)this.DataContext;
                vm.AddProductToModelsOrders(productId);
                items.Clear();
                RefreshProductsAndOrders(vm);
            }
        }
        /// <summary>
        /// Refresh product and order list on the view.
        /// </summary>
        /// <param name="vm">Viewmodel's refernc</param>
        /// <param name="refreshProduct">True if we want refresh products too</param>
        public void RefreshProductsAndOrders(ViewModelClass vm, bool refreshProduct = false)
        {
            items.Clear();
            if (vm.Model.Depo.Orders.Count != 0)
            {
                foreach (Order order in vm.Model.Depo.Orders)
                {
                    items.Add(order);
                }

            }
            if (refreshProduct)
            {
                products.Clear();
                foreach (int item in vm.Model.GetAllProducts())
                {
                    products.Add(new ProductsString { NameOfProduct = item + ". termék" });
                }
            }

            waitingList.ItemsSource = items;
            orderlist.ItemsSource = products;
            waitingList.Items.Refresh();
            orderlist.Items.Refresh();
        }
        /// <summary>
        /// Change the designation on the editors button.
        /// </summary>
        /// <param name="btnName">Represents the calling button's name</param>
        /// <param name="button">Represents the calling button</param>
        public void ChangeColor(String btnName, Button button)
        {
            Button tmp = this.FindName(btnName) as Button;
            designatedBtn = button;
            if (tmp != null)
            {
                tmp.BorderBrush = Brushes.Black;
            }
            if (button.Name == btnName)
            {
                tmp.BorderBrush = Brushes.Black;
            }
            else
            {
                if (button.BorderBrush == Brushes.Red)
                {
                    button.BorderBrush = Brushes.Black;
                }
                else if (button.BorderBrush == Brushes.Black)
                {
                    button.BorderBrush = Brushes.Red;
                }
            }
        }
        /// <summary>
        /// Generate the correct context menu for the right clicked button.
        /// </summary>
        /// <param name="btnType">Right clicked button's button type</param>
        /// <returns>Gives back a context menu to open</returns>
        public ContextMenu GenerateContextMenu(ButtonType btnType)
        {
            ContextMenu result = new ContextMenu();
            ViewModelClass vm = (ViewModelClass)this.DataContext;
            Separator sep = new Separator();
            MenuItem tmpMenuItem = new MenuItem();
            MenuItem delete = new MenuItem();
            delete.Header = "Elem törlése";
            delete.Click += new RoutedEventHandler(this.Delete);
            switch (btnType)
            {
                case ButtonType.Station:
                    result.Items.Add(delete);
                    break;
                case ButtonType.Robot:
                    int showedRobotsCount = vm.Model.RobotsListProp.Where(r => r.ShowingRoute).Count();
                    MenuItem details = new MenuItem();
                    details.Header = "Robot részletei";
                    details.Click += new RoutedEventHandler(this.ShowRobotData);
                    MenuItem showing = new MenuItem();
                    showing.Header = "Útvonal megjelenítése";
                    showing.Click += new RoutedEventHandler(this.ShowRoute);
                    if (showedRobotsCount == 2)
                    {
                        showing.IsEnabled = false;
                    }
                    if (vm.GetRobot(vm.rightClickedButton).ShowingRoute)
                    {
                        showing.IsEnabled = true;
                        showing.Header = "Útvonal elrejtése";
                    }
                    else
                    {
                        if (vm.GetRobot(vm.rightClickedButton).ShowingRoute)
                        {
                            showing.Header = "Útvonal elrejtése";
                        }
                    }
                    result.Items.Add(details);
                    result.Items.Add(showing);
                    result.Items.Add(sep);
                    result.Items.Add(delete);
                    break;
                case ButtonType.Charger:
                    result.Items.Add(delete);
                    break;
                case ButtonType.Shelf:
                    MenuItem products = new MenuItem();
                    products.Header = "Termékek módosítása";
                    products.Click += new RoutedEventHandler(this.ShelfProperties);
                    result.Items.Add(products);
                    result.Items.Add(sep);
                    result.Items.Add(delete);
                    break;
            }
            return result;
        }
        /// <summary>
        /// Open the given context menu.
        /// </summary>
        /// <param name="e">Right clicked button's button type</param>
        public void RightClick(ButtonType e)
        {
            if (e != ButtonType.None)
            {
                ContextMenu tmpCm = GenerateContextMenu(e);
                tmpCm.IsOpen = true;
            }
        }
        #endregion

    }
}
