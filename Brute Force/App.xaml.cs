using Brute_Force.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Brute_Force.Model;
using Brute_Force.ViewModel;
using Microsoft.Win32;
using Brute_Force.Persistence;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace Brute_Force
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private MainWindow _view; //main windows class
        private ViewModelClass _viewModel;// main windows datacontext
        private ModelClass _model;//the "brain" of the program
        private IDataAccess _access;//persistence layer's interface
        private DispatcherTimer _timer;//the timer which helps every seconds

        #endregion

        #region Methods
        /// <summary>
        /// Save every data about the program runnig, after run's end.
        /// </summary>
        private async void SaveSim()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Brute Force betöltése";
                saveFileDialog.Filter = "Brute Force|*.stl";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveStatistics(saveFileDialog.FileName);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to save game!" + Environment.NewLine + "The path is incorrect or the directory cannot be written.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to save game!", "Brute Force", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// The constructor of the App class, which connect the Startup event with App_Startup method
        /// </summary>
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }
        #endregion

        #region Events

        /// <summary>
        /// Startup event handler. Initialize main parts of program and activate main window. 
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void App_Startup(object sender, StartupEventArgs e)
        {
            _access = new DataAccess();
            _model = new ModelClass(12, 12, _access);
            _viewModel = new ViewModelClass(_model);
            _model.OrdersArrive += new EventHandler<EventArgs>(ArrivingOrder);
            //events of viewmodel
            ViewModelClass.LoadSimulation += new EventHandler(ViewModel_LoadGame);
            ViewModelClass.SaveTable += new EventHandler(ViewModel_SaveTable);
            ViewModelClass.RighClickEvent += new EventHandler<ButtonType>(ViewModel_Right);
            ViewModelClass.TimerEvent += new  EventHandler(ViewModel_StartStop);
            ViewModelClass.EndSimulation += new EventHandler(ViewModel_EndSim);
            ViewModelClass.RefreshOrders += new EventHandler(View_RefreshOrders);
            ViewModelClass.RefreshProductList += new EventHandler(View_RefreshProductList);
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            _view = new MainWindow(_timer, _viewModel);

            _view.Show();
            //events of view
            _view.ResizeEvent += new EventHandler<Coordinate>(RefreshWindow);
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing);

            _timer.Tick += new EventHandler(Timer_Tick);
        }

        private void ArrivingOrder(object sender, EventArgs e)
        {
            MessageBox.Show("A megrendelés megérkezett!", "Brute Force", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        /// <summary>
        /// Refreshing the product list of main view whent event is called.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void View_RefreshProductList(object sender, EventArgs e)
        {
            _view.RefreshProductsAndOrders(_viewModel,true);
        }

        /// <summary>
        /// Refreshing the orders lis of main view whent event is called.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void View_RefreshOrders(object sender, EventArgs e)
        {
            _view.RefreshProductsAndOrders(_viewModel);
        }

        /// <summary>
        /// Main window closing event handler with a closure message.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void View_Closing(object sender, CancelEventArgs e)
        {
            _timer.Stop();
            if (MessageBox.Show("Kívánja bezárni a programot?", "Brute Force", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; 
            }
        }


        
        /// <summary>
        /// Eventhandler of simulation end event. It handlers if we want to save status before quit, and the closure of the program.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void ViewModel_EndSim(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("A simuláció véget ért! Kívánja menteni a statisztikát?", "Brute Force", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if(result == MessageBoxResult.Yes)
            {
                SaveSim();
            }
            if(result == MessageBoxResult.No)
            {
                _view.Close();
            }
        }


        /// <summary>
        /// Reload main table with a new table size, which were given from model.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void RefreshWindow(object sender, Coordinate e)
        {
            _model = new ModelClass(e.X, e.Y, _access);
            _viewModel.GenerateWarehouse(_model);
            _view.designatedBtn.BorderBrush = Brushes.Black;
        }


        /// <summary>
        /// Start or Stop the program (it depens on, what is the program state) , when the event is executed.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void ViewModel_StartStop(object sender, EventArgs e)
        {
            if (_viewModel.StartStopContent == "Start")
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }
        }

        /// <summary>
        /// Runs every second, if the program's state is "running" and calls moveAtTick of model to step with the robots.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.MoveAtTick();
        }

        /// <summary>
        /// When user click on table with right click, it cause this event, which checks the program state, and if its running, drop a message.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private void ViewModel_Right(object sender, ButtonType e)
        {
            if(!_timer.IsEnabled)
                _view.RightClick(e);
            else
            {
                MessageBox.Show("Futás közben nem elérhetőek a tulajdonságok", "Brute Force", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// The load event handler. Call model's LoadDepot function, and loads the data from it into the program.
        /// </summary>
        /// <param name="sender">The object that calls the method</param>
        /// <param name="e">The actual event what cause this method calling</param>
        private async void ViewModel_LoadGame(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Brute Force betöltése";
                openFileDialog.Filter = "Brute Force|*.txt;*.matrix";
                if (openFileDialog.ShowDialog() == true)
                {
                    string ext = Path.GetExtension(openFileDialog.FileName);
                    await _model.LoadDepot(openFileDialog.FileName,ext);

                    _viewModel.GenerateWarehouse();
                    _view.RefreshProductsAndOrders(_viewModel,true);
                    _view.designatedBtn.BorderBrush = Brushes.Black;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Hiba a szimulációs állomány betöltésekor", "Brute Force", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        /// <summary>
        /// It gives acces to save the created table into a file for load it if its needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ViewModel_SaveTable(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Warehouse";
                saveFileDialog.Filter = "txt files|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveDepot(saveFileDialog.FileName);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Hiba a játék mentésekor!" + Environment.NewLine + "The path is incorrect or the directory cannot be written.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Hiba a játék mentésekor!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
