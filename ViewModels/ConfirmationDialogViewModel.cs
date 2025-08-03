using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using AccountManager.Models;
using AccountManager.Views.Base;
using AccountManager.ViewModels.Commands;
using MaterialDesignThemes.Wpf;

namespace AccountManager.ViewModels
{
    public enum ConfirmationType
    {
        Delete,
        Save,
        Discard,
        Override,
        Custom
    }

    public class ConfirmationDialogViewModel : BaseDialogViewModel
    {
        private string _inputText = "";
        private ConfirmationType _confirmationType = ConfirmationType.Custom;
        
        // For validation
        private string _expectedInput = "";
        private string _actualPassword = "";

        #region Display Properties

        public string Title { get; set; } = "Confirm Action";
        public string Subtitle { get; set; } = "";
        public string WarningMessage { get; set; } = "";
        public string InputLabel { get; set; } = "";
        public string InputHint { get; set; } = "";
        public string ConfirmButtonText { get; set; } = "Confirm";
        public string CancelButtonText { get; set; } = "Cancel";
        public PackIconKind IconKind { get; set; } = PackIconKind.Help;
        public Brush IconBackgroundColor { get; set; }
        public bool RequiresInput { get; set; } = false;
        public bool IsPasswordInput { get; set; } = false;
        public bool IsDestructive { get; set; } = false;
        public bool ShowChangesSummary { get; set; } = false;
        public List<string> ChangesList { get; set; } = new();

        #endregion

        public string InputText
        {
            get => _inputText;
            set
            {
                SetProperty(ref _inputText, value);
                ValidateData();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public override bool CanSave
        {
            get
            {
                if (!RequiresInput) return true;
                if (string.IsNullOrWhiteSpace(InputText)) return false;
                return !HasValidationError && !IsBusy;
            }
        }

        public ConfirmationType ConfirmationType
        {
            get => _confirmationType;
            set => SetProperty(ref _confirmationType, value);
        }

        public ConfirmationDialogViewModel()
        {
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(99, 102, 241)); // Default primary color
        }

        #region Setup Methods

        public void SetupForGroupDelete(AccountGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));

            ConfirmationType = ConfirmationType.Delete;
            Title = "Delete Group";
            Subtitle = "This action cannot be undone";
            
            if (group.Accounts.Count > 0)
            {
                WarningMessage = $"Group '{group.Name}' contains {group.Accounts.Count} account(s). All accounts will be permanently deleted.";
            }
            else
            {
                WarningMessage = $"Are you sure you want to delete the group '{group.Name}'?";
            }
            
            InputLabel = "Type the group name to confirm:";
            InputHint = group.Name;
            _expectedInput = group.Name;
            
            RequiresInput = true;
            IsPasswordInput = false;
            IsDestructive = true;
            IconKind = PackIconKind.FolderRemove;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Danger color
            ConfirmButtonText = "Delete Group";
            
            ValidateData();
        }

        public void SetupForAccountDelete(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            ConfirmationType = ConfirmationType.Delete;
            Title = "Delete Account";
            Subtitle = $"Delete '{account.Name}'";
            WarningMessage = "This action cannot be undone. Enter your account password to confirm deletion.";
            
            InputLabel = "Account password:";
            InputHint = "Enter password";
            _actualPassword = account.Password ?? "";
            
            RequiresInput = !string.IsNullOrEmpty(account.Password);
            IsPasswordInput = true;
            IsDestructive = true;
            IconKind = PackIconKind.AccountRemove;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Danger color
            ConfirmButtonText = "Delete Account";
            
            ValidateData();
        }

        public void SetupForAccountEdit(List<string> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            ConfirmationType = ConfirmationType.Save;
            Title = "Confirm Changes";
            Subtitle = "Save changes to account";
            WarningMessage = "";
            
            ChangesList = new List<string>(changes);
            ShowChangesSummary = true;
            
            RequiresInput = false;
            IsDestructive = false;
            IconKind = PackIconKind.ContentSave;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(99, 102, 241)); // Primary color
            ConfirmButtonText = "Save Changes";
            
            ValidateData();
        }

        public void SetupForUnsavedChanges()
        {
            ConfirmationType = ConfirmationType.Discard;
            Title = "Unsaved Changes";
            Subtitle = "You have unsaved changes";
            WarningMessage = "Are you sure you want to discard your changes? This action cannot be undone.";
            
            RequiresInput = false;
            IsDestructive = true;
            IconKind = PackIconKind.AlertCircle;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Warning color
            ConfirmButtonText = "Discard Changes";
            CancelButtonText = "Keep Editing";
            
            ValidateData();
        }

        public void SetupCustomConfirmation(string title, string message, bool isDestructive = false, 
            string confirmText = "Confirm", string cancelText = "Cancel", PackIconKind icon = PackIconKind.Help)
        {
            ConfirmationType = ConfirmationType.Custom;
            Title = title ?? "Confirm Action";
            Subtitle = "";
            WarningMessage = message ?? "";
            
            RequiresInput = false;
            IsDestructive = isDestructive;
            IconKind = icon;
            IconBackgroundColor = isDestructive 
                ? new SolidColorBrush(Color.FromRgb(239, 68, 68)) // Danger color
                : new SolidColorBrush(Color.FromRgb(99, 102, 241)); // Primary color
            ConfirmButtonText = confirmText;
            CancelButtonText = cancelText;
            
            ValidateData();
        }

        #endregion

        protected override void ValidateData()
        {
            ValidationError = "";

            if (!RequiresInput)
                return;

            if (string.IsNullOrWhiteSpace(InputText))
                return;

            // Group name validation
            if (!string.IsNullOrEmpty(_expectedInput))
            {
                if (InputText.Trim() != _expectedInput)
                {
                    ValidationError = "Group name does not match";
                }
            }
            // Password validation
            else if (!string.IsNullOrEmpty(_actualPassword))
            {
                if (InputText != _actualPassword)
                {
                    ValidationError = "Password is incorrect";
                }
            }
        }

        public string GetConfirmButtonStyle()
        {
            return IsDestructive ? "DangerButton" : "PrimaryButton";
        }

        public Brush GetIconColor()
        {
            return new SolidColorBrush(Colors.White);
        }

        public bool RequiresValidation => RequiresInput && (!string.IsNullOrEmpty(_expectedInput) || !string.IsNullOrEmpty(_actualPassword));
    }
}