using Brute_Force.Model;
using Brute_Force.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Brute_Force.View
{
    public partial class RobotData : Window
    {
        #region Fields
        private String robotTitle { get; set; } //The showing window's title
        private String robotImage { get; set; }//The robot images, which showes the direction too
        public Int32 robotEnergy { get; set; }//The current energy of the robot
        public Int32 robotUsedEnergy { get; set; }//The used energy
        public String robotShelf { get; set; } //The shelf which is connected to the robot

        private ViewModelClass viewModel;
        #endregion

        #region Properties
        public String RobotTitle
        {
            get { return robotTitle; }
            set
            {
                if (robotTitle != value)
                {
                    robotTitle = value;
                }
            }
        }

        public String RobotImage
        {
            get { return robotImage; }
            set
            {
                if (robotImage != value)
                {
                    robotImage = value;
                }
            }
        }

        public Int32 RobotEnergy
        {
            get { return robotEnergy; }
            set
            {
                if (robotEnergy != value)
                {
                    robotEnergy = value;
                }
            }
        }

        public Int32 RobotUsedEnergy
        {
            get { return robotUsedEnergy; }
            set
            {
                if (robotUsedEnergy != value)
                {
                    robotUsedEnergy = value;
                }
            }
        }
        public String RobotShelf
        {
            get { return robotShelf; }
            set
            {
                if (robotShelf != value)
                {
                    robotShelf = value;
                }
            }
        }
        #endregion



        #region Contstructor
        /// <summary>
        /// Construct the display of the window
        /// </summary>
        /// <param name="vm"></param>
        public RobotData(ViewModelClass vm)
        {
            this.DataContext = viewModel;
            InitializeComponent();
            viewModel = vm;
            Robot robot = viewModel.GetRobot(viewModel.rightClickedButton);
            String image = "";
            switch (robot.Direction)
            {
                case Direction.Up:
                    image = "/images/directions/robotUp.png";
                    break;
                case Direction.Down:
                    image = "/images/directions/robotDown.png";
                    break;
                case Direction.Left:
                    image = "/images/directions/robotLeft.png";
                    break;
                case Direction.Right:
                    image = "/images/directions/robotRight.png";
                    break;
            }
            RobotTitle = robot.Id + ". számú robot";
            RobotEnergy = robot.Energy;
            RobotUsedEnergy = robot.UsedEnergy;
            RobotImage = image;
            if (robot.DeliverdShelf.Position.Equals(new Coordinate(-1, -1)))
            {
                RobotShelf = "Nincs polc";
            }
            else
            {
                RobotShelf = robot.DeliverdShelf.Position.X + " " + robot.DeliverdShelf.Position.Y;
            }

        }
        #endregion


    }
}
