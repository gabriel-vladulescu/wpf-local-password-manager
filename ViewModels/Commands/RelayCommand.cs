using System;
using System.Windows.Input;

namespace AccountManager.ViewModels.Commands
{
    /// <summary>
    /// A command implementation that can invoke parameterless actions
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of RelayCommand
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
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
        /// <param name="parameter">Data used by the command (ignored in this implementation)</param>
        /// <returns>true if this command can be executed; otherwise, false</returns>
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command (ignored in this implementation)</param>
        public void Execute(object parameter) => _execute();
    }

    /// <summary>
    /// A command implementation that can invoke actions with typed parameters
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of RelayCommand with typed parameter
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
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
                return _canExecute?.Invoke(typedParameter) ?? true;

            // Handle null case for reference types
            if (parameter == null && !typeof(T).IsValueType)
                return _canExecute?.Invoke(default(T)) ?? true;

            return false;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
                return;
            }

            // Handle null case for reference types
            if (parameter == null && !typeof(T).IsValueType)
            {
                _execute(default(T));
                return;
            }

            throw new ArgumentException($"Parameter must be of type {typeof(T).Name}", nameof(parameter));
        }
    }

    /// <summary>
    /// A command implementation that can invoke actions with object parameters (non-generic version)
    /// </summary>
    public class RelayCommandWithParameter : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of RelayCommandWithParameter
        /// </summary>
        /// <param name="execute">The execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommandWithParameter(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
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
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command</param>
        public void Execute(object parameter) => _execute(parameter);
    }
}