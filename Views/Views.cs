using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using AccountManager.Models;
using AccountManager.Services;

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

        public DialogMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(IsCreateMode));
                OnPropertyChanged(nameof(IsViewMode)); 
                OnModeChanged();
            }
        }

        public bool IsEditMode => Mode == DialogMode.Edit;
        public bool IsCreateMode => Mode == DialogMode.Create;
        public bool IsViewMode => Mode == DialogMode.View;

        /// <summary>
        /// Called when the dialog mode changes
        /// </summary>
        protected virtual void OnModeChanged() { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                firstTextBox.Focus();
                
                // Select all text if in edit mode for easy overwriting
                if (DataContext is BaseDialogViewModel vm && vm.IsEditMode)
                {
                    firstTextBox.SelectAll();
                }
            }
        }
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
                _groupName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
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

                _account = value;

                if (_account != null)
                    _account.PropertyChanged += Account_PropertyChanged;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
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
            // Refresh computed properties when mode changes
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(IconKind));
        }
    }

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
            // Settings are automatically loaded from service
            OnPropertyChanged(nameof(CensorAccountData));
            OnPropertyChanged(nameof(CensorPassword));
            OnPropertyChanged(nameof(EnableEncryption));
            OnPropertyChanged(nameof(EnableLocalSearch));
        }
    }

    /// <summary>
    /// Simple relay command implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
    }

    /// <summary>
    /// Helper methods for dialog operations
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Find the first visible and enabled TextBox in a visual tree
        /// </summary>
        public static TextBox FindFirstTextBox(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox && textBox.IsVisible && textBox.IsEnabled)
                {
                    return textBox;
                }

                var result = FindFirstTextBox(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Show a dialog and return the result using custom dialog service
        /// </summary>
        public static async System.Threading.Tasks.Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            try
            {
                return await DialogService.ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Extension methods for views
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Setup common dialog behavior
        /// </summary>
        public static void SetupDialogBehavior(this BaseDialog dialog)
        {
            // Any common setup logic can go here
        }
    }
}