using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Brute_Force.ViewModel
{
    /// <summary>
    /// General command's type.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<Object> _execute;
        private readonly Func<Object, Boolean> _canExecute;

        /// <summary>
        /// Create command
        /// </summary>
        /// <param name="execute">Activity to be carried out</param>
        public DelegateCommand(Action<Object> execute) : this(null, execute) { }
        /// <summary>
        /// Create command
        /// </summary>
        /// <param name="canExecute">Condition of enforceability.</param>
        /// <param name="execute">Activity to be carried out</param>
        public DelegateCommand(Func<Object, Boolean> canExecute, Action<Object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }
        /// <summary>
        /// Event of change in enforceability.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Verification of enforceability
        /// </summary>
        /// <param name="parameter">Activity parameter.</param>
        /// <returns>True if the activity is executable.</returns>
        public Boolean CanExecute(Object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        /// <summary>
        /// Execution of an activity.
        /// </summary>
        /// <param name="parameter">Activity parameter.</param>
        public void Execute(Object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Command execution is disabled.");
            }
            _execute(parameter);
        }
    }
}
