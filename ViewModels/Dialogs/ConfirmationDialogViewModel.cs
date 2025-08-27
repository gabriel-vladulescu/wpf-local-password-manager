using System;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class ConfirmationDialogViewModel : BaseViewModel
    {
        public event EventHandler CloseRequested;
        private string _title = "Confirm Action";
        private string _message = "Are you sure you want to perform this action?";
        private string _icon = "HelpCircle";
        private string _iconColor = "#6366F1";
        private string _confirmText = "Confirm";
        private string _cancelText = "Cancel";
        private string _inputText = "";
        private string _inputHint = "";
        private bool _showInput = false;
        private bool _requireConfirmation = false;
        private string _confirmationText = "";
        private bool _result = false;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string IconColor
        {
            get => _iconColor;
            set => SetProperty(ref _iconColor, value);
        }

        public string ConfirmText
        {
            get => _confirmText;
            set => SetProperty(ref _confirmText, value);
        }

        public string CancelText
        {
            get => _cancelText;
            set => SetProperty(ref _cancelText, value);
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                if (SetProperty(ref _inputText, value))
                {
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        public string InputHint
        {
            get => _inputHint;
            set => SetProperty(ref _inputHint, value);
        }

        public bool ShowInput
        {
            get => _showInput;
            set => SetProperty(ref _showInput, value);
        }

        public bool RequireConfirmation
        {
            get => _requireConfirmation;
            set
            {
                if (SetProperty(ref _requireConfirmation, value))
                {
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        public string ConfirmationText
        {
            get => _confirmationText;
            set => SetProperty(ref _confirmationText, value);
        }

        public bool Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public bool CanConfirm
        {
            get
            {
                if (RequireConfirmation && !string.Equals(InputText, ConfirmationText, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (ShowInput && string.IsNullOrWhiteSpace(InputText))
                    return false;

                return true;
            }
        }

        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ConfirmationDialogViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ConfirmCommand = new RelayCommand(Confirm, _ => CanConfirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void SetupForAccountTrash(Account account)
        {
            Title = "Move to Trash";
            Message = $"Move '{account.Name}' to trash?";
            Icon = "Delete";
            IconColor = "#F59E0B"; // Orange
            ConfirmText = "Move to Trash";
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
        }

        public void SetupForEmptyTrash(int itemCount)
        {
            Title = "Empty Trash";
            Message = $"Delete {itemCount} items permanently? This action cannot be undone.";
            Icon = "Delete";
            IconColor = "#EF4444"; // Red
            ConfirmText = "Empty Trash";
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
        }

        public void SetupForGroupDelete(AccountGroup group)
        {
            Title = "Delete Group";
            Message = $"Delete '{group.Name}' and all its accounts?";
            Icon = "Delete";
            IconColor = "#EF4444"; // Red
            ConfirmText = "Delete";
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
        }

        public void SetupForAccountDelete(Account account)
        {
            Title = "Delete Account";
            Message = $"Delete '{account.Name}' permanently? This action cannot be undone.";
            Icon = "Delete";
            IconColor = "#EF4444"; // Red
            ConfirmText = "Delete";
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
        }

        public void SetupForAccountArchive(Account account)
        {
            Title = "Archive Account";
            Message = $"Archive '{account.Name}'? It will be moved to the archive section.";
            Icon = "Archive";
            IconColor = "#8B5CF6"; // Purple
            ConfirmText = "Archive";
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
        }

        public void SetupForGenericAction(string title, string message, string confirmText = "Confirm")
        {
            Title = title;
            Message = message;
            Icon = "HelpCircle";
            IconColor = "#6366F1"; // Blue
            ConfirmText = confirmText;
            CancelText = "Cancel";
            ShowInput = false;
            RequireConfirmation = false;
            InputText = "";
            ConfirmationText = "";
        }

        public void SetupForTextInput(string title, string message, string hint, string confirmText = "OK")
        {
            Title = title;
            Message = message;
            InputHint = hint;
            Icon = "TextBox";
            IconColor = "#6366F1"; // Blue
            ConfirmText = confirmText;
            CancelText = "Cancel";
            ShowInput = true;
            RequireConfirmation = false;
            InputText = "";
            ConfirmationText = "";
        }

        private void Confirm(object parameter)
        {
            Result = true;
            RequestClose();
        }

        private void Cancel(object parameter)
        {
            Result = false;
            RequestClose();
        }

        // NEW: Add the missing methods that existing code expects
        public void SetupGroupDeleteConfirmation(AccountGroup group)
        {
            SetupForGroupDelete(group);
        }

        public void SetupAccountDeleteConfirmation(Account account)
        {
            SetupForAccountDelete(account);
        }

        public void SetupAccountEditConfirmation(Account account)
        {
            SetupForGenericAction("Edit Account", $"Edit account '{account.Name}'?", "Edit");
        }

        // Add RequestClose method
        public void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}