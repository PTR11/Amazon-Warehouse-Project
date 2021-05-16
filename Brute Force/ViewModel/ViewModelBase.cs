using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Brute_Force.ViewModel
{
    /// <summary>
    /// Viewmodel's parent class.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Initialize Viewmodel's parent class.
        /// </summary>
        protected ViewModelBase() { }
        /// <summary>
        /// Event of the some property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Event of the some property changes with checks.
        /// </summary>
        /// <param name="propertyName">Property's name.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
