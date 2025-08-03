using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using AccountManager.Models;
using AccountManager.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Views.Dialogs
{
    public class ConfirmationDialogViewModel : INotifyPropertyChanged
    {
        private string _inputText = "";
        private string _errorMessage = "";
        private bool _result = false;

        // Event to notify when dialog should close
        public event EventHandler<bool> RequestClose;

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string WarningMessage { get; set; }
        public string InputLabel { get; set; }
        public string InputHint { get; set; }
        public string ConfirmButtonText { get; set; } = "Confirm";
        public string CancelButtonText { get; set; } = "Cancel";
        public PackIconKind IconKind { get; set; }
        public Brush IconBackgroundColor { get; set; }
        public bool RequiresInput { get; set; }
        public bool IsPasswordInput { get; set; }
        public bool IsDestructive { get; set; }
        public bool ShowChangesSummary { get; set; }
        public List<string> ChangesList { get; set; } = new();

        // For validation
        private string _expectedInput = "";
        private string _actualPassword = "";

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
                ValidateInput();
                OnPropertyChanged(nameof(CanConfirm));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanConfirm
        {
            get
            {
                if (!RequiresInput) return true;
                if (string.IsNullOrWhiteSpace(InputText)) return false;
                return string.IsNullOrEmpty(ErrorMessage);
            }
        }

        public bool Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ConfirmationDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        #region Setup Methods

        public void SetupGroupDeleteConfirmation(AccountGroup group)
        {
            Title = "Delete Group";
            Subtitle = $"This action cannot be undone";
            WarningMessage = group.Accounts.Count > 0 
                ? $"Group '{group.Name}' contains {group.Accounts.Count} account(s). All accounts will be permanently deleted."
                : $"Are you sure you want to delete the group '{group.Name}'?";
            
            InputLabel = "Type the group name to confirm:";
            InputHint = group.Name;
            _expectedInput = group.Name;
            
            RequiresInput = true;
            IsPasswordInput = false;
            IsDestructive = true;
            IconKind = PackIconKind.FolderRemove;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // DangerColor
            ConfirmButtonText = "Delete Group";
        }

        public void SetupAccountDeleteConfirmation(Account account)
        {
            Title = "Delete Account";
            Subtitle = $"Delete '{account.Name}'";
            WarningMessage = "This action cannot be undone. Enter your account password to confirm deletion.";
            
            InputLabel = "Account password:";
            InputHint = "Enter password";
            _actualPassword = account.Password;
            
            RequiresInput = true;
            IsPasswordInput = true;
            IsDestructive = true;
            IconKind = PackIconKind.AccountRemove;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // DangerColor
            ConfirmButtonText = "Delete Account";
        }

        public void SetupAccountEditConfirmation(List<string> changes)
        {
            Title = "Confirm Changes";
            Subtitle = "Save changes to account";
            WarningMessage = "";
            
            ChangesList = changes;
            ShowChangesSummary = true;
            
            RequiresInput = false;
            IsDestructive = false;
            IconKind = PackIconKind.ContentSave;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(99, 102, 241)); // PrimaryColor
            ConfirmButtonText = "Save Changes";
        }

        #endregion

        private void ValidateInput()
        {
            if (!RequiresInput)
            {
                ErrorMessage = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(InputText))
            {
                ErrorMessage = "";
                return;
            }

            // Group name validation
            if (!string.IsNullOrEmpty(_expectedInput))
            {
                if (InputText.Trim() != _expectedInput)
                {
                    ErrorMessage = "Group name not valid";
                }
                else
                {
                    ErrorMessage = "";
                }
            }
            // Password validation
            else if (!string.IsNullOrEmpty(_actualPassword))
            {
                if (InputText != _actualPassword)
                {
                    ErrorMessage = "Password is incorrect";
                }
                else
                {
                    ErrorMessage = "";
                }
            }
            
            // Force command to re-evaluate CanExecute
            CommandManager.InvalidateRequerySuggested();
        }

        private void Confirm()
        {
            if (CanConfirm)
            {
                Result = true;
                RequestClose?.Invoke(this, true);
            }
        }

        private void Cancel()
        {
            Result = false;
            RequestClose?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}