using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Views.Base;
using AccountManager.ViewModels.Commands;

namespace AccountManager.Views
{
    /// <summary>
    /// Dialog operation modes
    /// </summary>
    public enum DialogMode
    {
        Create,
        Edit,
        View
    }

    /// <summary>
    /// Base class for all dialog view models
    /// </summary>
    public abstract class BaseDialogViewModel : INotifyPropertyChanged
    {
        private DialogMode _mode = DialogMode.Create;
        private bool _isLoading = false;
        private string _validationError = "";

        public DialogMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(IsCreateMode));
                    OnPropertyChanged(nameof(IsViewMode));
                    OnModeChanged();
                }
            }
        }

        public bool IsEditMode => Mode == DialogMode.Edit;
        public bool IsCreateMode => Mode == DialogMode.Create;
        public bool IsViewMode => Mode == DialogMode.View;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public string ValidationError
        {
            get => _validationError;
            set
            {
                if (_validationError != value)
                {
                    _validationError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasValidationError));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);

        /// <summary>
        /// Override this property to define save validation logic
        /// </summary>
        public abstract bool CanSave { get; }

        /// <summary>
        /// Called when the dialog mode changes
        /// </summary>
        protected virtual void OnModeChanged() { }

        /// <summary>
        /// Validate the current state and set ValidationError if needed
        /// </summary>
        protected abstract void ValidateData();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }

    /// <summary>
    /// Base class for all dialog user controls
    /// </summary>
    public abstract class BaseDialog : UserControl
    {
        protected BaseDialog()
        {
            Loaded += OnDialogLoaded;
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus the first input field when dialog opens
            var firstTextBox = DialogHelper.FindFirstTextBox(this);
            if (firstTextBox != null)
            {
                // Use Dispatcher to ensure UI is fully loaded
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    firstTextBox.Focus();
                    
                    // Select all text if in edit mode for easy overwriting
                    if (DataContext is BaseDialogViewModel vm && vm.IsEditMode)
                    {
                        firstTextBox.SelectAll();
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// Virtual method that can be overridden to set up dialog-specific behavior
        /// </summary>
        protected virtual void SetupDialogBehavior() { }
    }
}