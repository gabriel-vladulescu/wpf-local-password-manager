using System;
using System.Windows.Input;

namespace AccountManager.ViewModels.Commands
{
    /// <summary>
    /// Alternative command implementation using delegates
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        private readonly Predicate<object> _canExecuteAction;

        /// <summary>
        /// Initializes a new instance of DelegateCommand
        /// </summary>
        /// <param name="executeAction">The execution logic</param>
        /// <param name="canExecuteAction">The execution status logic</param>
        public DelegateCommand(Action<object> executeAction, Predicate<object> canExecuteAction = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        /// Initializes a new instance of DelegateCommand with parameterless action
        /// </summary>
        /// <param name="executeAction">The execution logic</param>
        /// <param name="canExecuteAction">The execution status logic</param>
        public DelegateCommand(Action executeAction, Func<bool> canExecuteAction = null)
        {
            if (executeAction == null) throw new ArgumentNullException(nameof(executeAction));
            
            _executeAction = _ => executeAction();
            _canExecuteAction = canExecuteAction != null ? _ => canExecuteAction() : null;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        /// <returns>true if this command can be executed; otherwise, false</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _executeAction(parameter);
            }
        }

        /// <summary>
        /// Raise the CanExecuteChanged event manually
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Generic delegate command with typed parameters
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Predicate<T> _canExecuteAction;

        /// <summary>
        /// Initializes a new instance of DelegateCommand with typed parameter
        /// </summary>
        /// <param name="executeAction">The execution logic</param>
        /// <param name="canExecuteAction">The execution status logic</param>
        public DelegateCommand(Action<T> executeAction, Predicate<T> canExecuteAction = null)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        /// <returns>true if this command can be executed; otherwise, false</returns>
        public bool CanExecute(object parameter)
        {
            if (parameter is T typedParameter)
                return _canExecuteAction?.Invoke(typedParameter) ?? true;

            // Handle null case for reference types
            if (parameter == null && !typeof(T).IsValueType)
                return _canExecuteAction?.Invoke(default(T)) ?? true;

            return false;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            if (parameter is T typedParameter)
            {
                _executeAction(typedParameter);
            }
            else if (parameter == null && !typeof(T).IsValueType)
            {
                _executeAction(default(T));
            }
        }

        /// <summary>
        /// Raise the CanExecuteChanged event manually
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}