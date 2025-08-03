using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AccountManager.ViewModels;
using AccountManager.ViewModels.Commands;

namespace AccountManager.Views.Base
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
    public abstract class BaseDialogViewModel : BaseViewModel
    {
        private DialogMode _mode = DialogMode.Create;
        private string _validationError = "";
        private bool _dialogResult = false;

        #region Properties

        public DialogMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnModeChanged();
                    UpdateComputedProperties();
                }
            }
        }

        public bool IsEditMode => Mode == DialogMode.Edit;
        public bool IsCreateMode => Mode == DialogMode.Create;
        public bool IsViewMode => Mode == DialogMode.View;

        public string ValidationError
        {
            get => _validationError;
            set
            {
                if (SetProperty(ref _validationError, value))
                {
                    OnPropertyChanged(nameof(HasValidationError));
                    OnPropertyChanged(nameof(CanSave));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);

        public bool DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        /// <summary>
        /// Override this property to define save validation logic
        /// </summary>
        public abstract bool CanSave { get; }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event to notify when dialog should close
        /// </summary>
        public event EventHandler<bool> RequestClose;

        #endregion

        #region Constructor

        protected BaseDialogViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => CanSave && !IsBusy);
            CancelCommand = new RelayCommand(Cancel);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Validate the current state and set ValidationError if needed
        /// </summary>
        protected abstract void ValidateData();

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called when the dialog mode changes
        /// </summary>
        protected virtual void OnModeChanged() { }

        /// <summary>
        /// Called when the save command is executed
        /// </summary>
        protected virtual void OnSave() { }

        /// <summary>
        /// Called when the cancel command is executed
        /// </summary>
        protected virtual void OnCancel() { }

        #endregion

        #region Private Methods

        private void Save()
        {
            if (!CanSave || IsBusy) return;

            try
            {
                ValidateData();
                if (HasValidationError) return;

                OnSave();
                DialogResult = true;
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
            }
        }

        private void Cancel()
        {
            try
            {
                OnCancel();
                DialogResult = false;
                RequestClose?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                ShowError($"Cancel failed: {ex.Message}");
            }
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(IsCreateMode));
            OnPropertyChanged(nameof(IsViewMode));
            OnPropertyChanged(nameof(CanSave));
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Protected Overrides

        protected override void OnIsBusyChanged()
        {
            base.OnIsBusyChanged();
            OnPropertyChanged(nameof(CanSave));
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}