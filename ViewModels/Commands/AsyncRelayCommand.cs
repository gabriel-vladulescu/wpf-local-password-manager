
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccountManager.ViewModels.Commands
{
    /// <summary>
    /// An async command implementation that can invoke async parameterless actions
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting = false;

        /// <summary>
        /// Initializes a new instance of AsyncRelayCommand
        /// </summary>
        /// <param name="execute">The async execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
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
        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        /// <summary>
        /// Defines the method to be called when the command is invoked
        /// </summary>
        /// <param name="parameter">Data used by the command (ignored in this implementation)</param>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute();
            }
            catch (Exception ex)
            {
                // Log error but don't rethrow to prevent unhandled exceptions
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand error: {ex.Message}");
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raise the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// An async command implementation that can invoke async actions with typed parameters
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting = false;

        /// <summary>
        /// Initializes a new instance of AsyncRelayCommand with typed parameter
        /// </summary>
        /// <param name="execute">The async execution logic</param>
        /// <param name="canExecute">The execution status logic</param>
        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
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
            if (_isExecuting) return false;

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
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                if (parameter is T typedParameter)
                {
                    await _execute(typedParameter);
                }
                else if (parameter == null && !typeof(T).IsValueType)
                {
                    await _execute(default(T));
                }
            }
            catch (Exception ex)
            {
                // Log error but don't rethrow to prevent unhandled exceptions
                System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand<{typeof(T).Name}> error: {ex.Message}");
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raise the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}