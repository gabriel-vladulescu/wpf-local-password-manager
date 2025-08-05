using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using AccountManager.Commands;
using AccountManager.Models;
using AccountManager.Services;
using MaterialDesignThemes.Wpf;

namespace AccountManager.ViewModels
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

        public DialogMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertiesChanged(nameof(IsEditMode), nameof(IsCreateMode), nameof(IsViewMode));
                    OnModeChanged();
                }
            }
        }

        public bool IsEditMode => Mode == DialogMode.Edit;
        public bool IsCreateMode => Mode == DialogMode.Create;
        public bool IsViewMode => Mode == DialogMode.View;

        /// <summary>
        /// Called when the dialog mode changes
        /// </summary>
        protected virtual void OnModeChanged() { }
    }

    /// <summary>
    /// ViewModel for Group dialog (both create and edit modes)
    /// </summary>
    public class GroupDialogViewModel : BaseDialogViewModel
    {
        private string _groupName = "";
        private AccountGroup _originalGroup;

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (SetProperty(ref _groupName, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(GroupName);

        public string Title => IsEditMode ? "Edit Group" : "Create New Group";
        public string Subtitle => IsEditMode ? "Update your group information" : "Organize your accounts with a new group";
        public string ActionButtonText => IsEditMode ? "Save Changes" : "Create Group";
        public string IconKind => IsEditMode ? "FolderEdit" : "FolderPlus";

        /// <summary>
        /// Initialize for create mode
        /// </summary>
        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            GroupName = "";
            _originalGroup = null;
        }

        /// <summary>
        /// Initialize for edit mode
        /// </summary>
        public void InitializeForEdit(AccountGroup group)
        {
            Mode = DialogMode.Edit;
            _originalGroup = group;
            GroupName = group.Name;
        }

        /// <summary>
        /// Apply changes to the original group (for edit mode)
        /// </summary>
        public void ApplyChanges()
        {
            if (IsEditMode && _originalGroup != null)
            {
                _originalGroup.Name = GroupName;
            }
        }

        /// <summary>
        /// Create a new group (for create mode)
        /// </summary>
        public AccountGroup CreateGroup()
        {
            if (IsCreateMode)
            {
                return new AccountGroup { Name = GroupName };
            }
            return null;
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertiesChanged(nameof(Title), nameof(Subtitle), nameof(ActionButtonText), nameof(IconKind));
        }
    }

    /// <summary>
    /// ViewModel for Account dialog (both create and edit modes)
    /// </summary>
    public class AccountDialogViewModel : BaseDialogViewModel
    {
        private Account _account = new();
        private Account _originalAccount;

        public Account Account
        {
            get => _account;
            set
            {
                if (_account != null)
                    _account.PropertyChanged -= Account_PropertyChanged;

                if (SetProperty(ref _account, value))
                {
                    if (_account != null)
                        _account.PropertyChanged += Account_PropertyChanged;

                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(Account?.Name);

        public string Title => IsEditMode ? "Edit Account" : "Create Account";
        public string Subtitle => IsEditMode ? "Update your account information" : "Add a new account to your collection";
        public string ActionButtonText => IsEditMode ? "Update Account" : "Create Account";
        public string IconKind => IsEditMode ? "AccountEdit" : "AccountPlus";

        public ICommand GeneratePasswordCommand { get; }

        public AccountDialogViewModel()
        {
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
            Account = new Account();
        }

        /// <summary>
        /// Initialize for create mode
        /// </summary>
        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            Account = new Account();
            _originalAccount = null;
        }

        /// <summary>
        /// Initialize for edit mode
        /// </summary>
        public void InitializeForEdit(Account account)
        {
            Mode = DialogMode.Edit;
            _originalAccount = account;
            
            // Create a copy to avoid direct binding to the original
            Account = new Account
            {
                Name = account.Name,
                Username = account.Username,
                Email = account.Email,
                Password = account.Password,
                Website = account.Website,
                Notes = account.Notes
            };
        }

        /// <summary>
        /// Apply changes to the original account (for edit mode)
        /// </summary>
        public void ApplyChanges()
        {
            if (IsEditMode && _originalAccount != null)
            {
                _originalAccount.Name = Account.Name;
                _originalAccount.Username = Account.Username;
                _originalAccount.Email = Account.Email;
                _originalAccount.Password = Account.Password;
                _originalAccount.Website = Account.Website;
                _originalAccount.Notes = Account.Notes;
            }
        }

        /// <summary>
        /// Create a new account (for create mode)
        /// </summary>
        public Account CreateAccount()
        {
            if (IsCreateMode)
            {
                return new Account
                {
                    Name = Account.Name,
                    Username = Account.Username,
                    Email = Account.Email,
                    Password = Account.Password,
                    Website = Account.Website,
                    Notes = Account.Notes
                };
            }
            return null;
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Account.Name))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private void GeneratePassword(object parameter)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            Account.Password = password;
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertiesChanged(nameof(Title), nameof(Subtitle), nameof(ActionButtonText), nameof(IconKind));
        }
    }

    /// <summary>
    /// ViewModel for Settings dialog
    /// </summary>
    public class SettingsDialogViewModel : BaseDialogViewModel
    {
        private readonly SettingsService _settingsService;

        public SettingsDialogViewModel()
        {
            _settingsService = SettingsService.Instance;
        }

        public bool CensorAccountData
        {
            get => _settingsService.CensorAccountData;
            set => _settingsService.CensorAccountData = value;
        }

        public bool CensorPassword
        {
            get => _settingsService.CensorPassword;
            set => _settingsService.CensorPassword = value;
        }

        public bool EnableEncryption
        {
            get => _settingsService.EnableEncryption;
            set => _settingsService.EnableEncryption = value;
        }

        public bool EnableLocalSearch
        {
            get => _settingsService.EnableLocalSearch;
            set => _settingsService.EnableLocalSearch = value;
        }

        public void InitializeForView()
        {
            Mode = DialogMode.View;
            OnPropertiesChanged(
                nameof(CensorAccountData),
                nameof(CensorPassword),
                nameof(EnableEncryption),
                nameof(EnableLocalSearch)
            );
        }
    }

    /// <summary>
    /// ViewModel for Confirmation dialog
    /// </summary>
    public class ConfirmationDialogViewModel : BaseViewModel
    {
        private string _inputText = "";
        private string _errorMessage = "";
        private bool _result = false;
        private string _expectedInput = "";
        private string _actualPassword = "";

        public event EventHandler<bool> RequestClose;

        // Display Properties
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

        public string InputText
        {
            get => _inputText;
            set
            {
                if (SetProperty(ref _inputText, value))
                {
                    ValidateInput();
                    OnPropertyChanged(nameof(CanConfirm));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(CanConfirm));
                }
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
            set => SetProperty(ref _result, value);
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ConfirmationDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        public void SetupGroupDeleteConfirmation(AccountGroup group)
        {
            Title = "Delete Group";
            Subtitle = "This action cannot be undone";
            WarningMessage = group.Accounts.Count > 0 
                ? $"Group '{group.Name}' contains {group.Accounts.Count} account(s). All accounts will be permanently deleted."
                : $"Are you sure you want to delete the group '{group.Name}'?";
            
            InputLabel = "Type the group name to confirm:";
            InputHint = group.Name;
            _expectedInput = group.Name;
            
            RequiresInput = true;
            IsDestructive = true;
            IconKind = PackIconKind.FolderRemove;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
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
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            ConfirmButtonText = "Delete Account";
        }

        public void SetupAccountEditConfirmation(List<string> changes)
        {
            Title = "Confirm Changes";
            Subtitle = "Save changes to account";
            ChangesList = changes;
            ShowChangesSummary = true;
            IsDestructive = false;
            IconKind = PackIconKind.ContentSave;
            IconBackgroundColor = new SolidColorBrush(Color.FromRgb(99, 102, 241));
            ConfirmButtonText = "Save Changes";
        }

        private void ValidateInput()
        {
            if (!RequiresInput || string.IsNullOrWhiteSpace(InputText))
            {
                ErrorMessage = "";
                return;
            }

            if (!string.IsNullOrEmpty(_expectedInput))
            {
                ErrorMessage = InputText.Trim() != _expectedInput ? "Group name not valid" : "";
            }
            else if (!string.IsNullOrEmpty(_actualPassword))
            {
                ErrorMessage = InputText != _actualPassword ? "Password is incorrect" : "";
            }
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
    }

    // Legacy ViewModels for backward compatibility (can be removed eventually)
    [Obsolete("Use AccountDialogViewModel instead")]
    public class EditAccountViewModel : BaseViewModel
    {
        private Account _account = new();
        
        public Account Account 
        { 
            get => _account;
            set 
            { 
                if (_account != null)
                    _account.PropertyChanged -= Account_PropertyChanged;
                    
                if (SetProperty(ref _account, value))
                {
                    if (_account != null)
                        _account.PropertyChanged += Account_PropertyChanged;
                        
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }
        
        public bool IsEditing { get; set; }
        public bool CanSave => !string.IsNullOrWhiteSpace(Account?.Name);
        public ICommand GeneratePasswordCommand { get; }

        public EditAccountViewModel()
        {
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
            Account = new Account();
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Account.Name))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private void GeneratePassword(object parameter)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            Account.Password = password;
        }
    }

    [Obsolete("Use GroupDialogViewModel instead")]
    public class AddGroupViewModel : BaseViewModel
    {
        private string _groupName = "";

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (SetProperty(ref _groupName, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(GroupName);
    }
}