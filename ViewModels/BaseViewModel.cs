using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AccountManager.ViewModels.Commands; // Add this import

namespace AccountManager.ViewModels
{
    /// <summary>
    /// Base class for all view models providing common functionality
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isBusy = false;
        private string _busyMessage = "";
        private string _errorMessage = "";
        private bool _hasError = false;
        private bool _isInitialized = false;
        private bool _isDisposed = false;

        #region Properties

        /// <summary>
        /// Indicates if the view model is currently performing an operation
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            protected set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    OnIsBusyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Message to display while busy
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            protected set => SetProperty(ref _busyMessage, value);
        }

        /// <summary>
        /// Current error message
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            protected set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Indicates if there's an active error
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            protected set => SetProperty(ref _hasError, value);
        }

        /// <summary>
        /// Indicates if the view model has been initialized
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            protected set => SetProperty(ref _isInitialized, value);
        }

        /// <summary>
        /// Indicates if the view model has been disposed
        /// </summary>
        public bool IsDisposed
        {
            get => _isDisposed;
            private set => _isDisposed = value;
        }

        #endregion

        #region Commands

        public ICommand ClearErrorCommand { get; }

        #endregion

        #region Constructor

        protected BaseViewModel()
        {
            ClearErrorCommand = new RelayCommand(ClearError);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the view model
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            if (IsInitialized || IsDisposed) return;

            try
            {
                await OnInitializeAsync();
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                ShowError($"Initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Refresh the view model data
        /// </summary>
        public virtual async Task RefreshAsync()
        {
            if (!IsInitialized || IsDisposed) return;

            try
            {
                await OnRefreshAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Refresh failed: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Override this method to perform initialization logic
        /// </summary>
        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to perform refresh logic
        /// </summary>
        protected virtual Task OnRefreshAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when IsBusy property changes
        /// </summary>
        protected virtual void OnIsBusyChanged()
        {
            // Override in derived classes if needed
        }

        /// <summary>
        /// Execute an async operation with busy state management
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> operation, string busyMessage = "Working...")
        {
            if (IsBusy || IsDisposed) return;

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                ClearError();

                await operation();
            }
            catch (Exception ex)
            {
                ShowError($"Operation failed: {ex.Message}");
                OnError(ex);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = "";
            }
        }

        /// <summary>
        /// Execute an async operation with busy state management and return result
        /// </summary>
        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string busyMessage = "Working...")
        {
            if (IsBusy || IsDisposed) return default(T);

            try
            {
                IsBusy = true;
                BusyMessage = busyMessage;
                ClearError();

                return await operation();
            }
            catch (Exception ex)
            {
                ShowError($"Operation failed: {ex.Message}");
                OnError(ex);
                return default(T);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = "";
            }
        }

        /// <summary>
        /// Show an error message
        /// </summary>
        protected void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = !string.IsNullOrEmpty(message);
            OnErrorShown(message);
        }

        /// <summary>
        /// Clear the current error
        /// </summary>
        protected void ClearError()
        {
            ErrorMessage = "";
            HasError = false;
        }

        /// <summary>
        /// Called when an error occurs during operations
        /// </summary>
        protected virtual void OnError(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error: {exception.Message}");
        }

        /// <summary>
        /// Called when an error message is shown
        /// </summary>
        protected virtual void OnErrorShown(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Error shown: {message}");
        }

        /// <summary>
        /// Validate that the view model is not disposed
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                onChanged?.Invoke();
                return true;
            }
            return false;
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                try
                {
                    OnDisposing();
                    PropertyChanged = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
                }
                finally
                {
                    IsDisposed = true;
                }
            }
        }

        /// <summary>
        /// Override this method to perform cleanup
        /// </summary>
        protected virtual void OnDisposing()
        {
            // Override in derived classes for cleanup
        }

        ~BaseViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}