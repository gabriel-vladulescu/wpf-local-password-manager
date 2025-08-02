using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Views;
using AccountManager.Views.Dialogs;
using MaterialDesignThemes.Wpf;

namespace AccountManager.ViewModels
{
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

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly JsonService _jsonService;
        private AccountGroup _selectedGroup;
        private string _searchText = "";
        private ObservableCollection<Account> _filteredAccounts = new();
        private string _errorMessage = "";
        private bool _hasError = false;

        public ObservableCollection<AccountGroup> Groups { get; set; } = new();

        public ThemeService ThemeService => ThemeService.Instance;

        public AccountGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmptyStateVisible));
                OnPropertyChanged(nameof(IsNoGroupsStateVisible));
                OnPropertyChanged(nameof(IsAccountsPanelVisible));
                UpdateFilteredAccounts();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                UpdateFilteredAccounts();
            }
        }

        public ObservableCollection<Account> FilteredAccounts
        {
            get => _filteredAccounts;
            set
            {
                _filteredAccounts = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public bool IsEmptyStateVisible => SelectedGroup == null && Groups.Any();
        public bool IsNoGroupsStateVisible => !Groups.Any();
        public bool IsAccountsPanelVisible => SelectedGroup != null;

        public ICommand AddGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand SelectGroupCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand CopyEmailCommand { get; }
        public ICommand CopyUsernameCommand { get; }
        public ICommand CopyPasswordCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand DismissErrorCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        public MainViewModel()
        {
            _jsonService = new JsonService();
            
            AddGroupCommand = new RelayCommand(AddGroup);
            EditGroupCommand = new RelayCommand(EditGroup);
            DeleteGroupCommand = new RelayCommand(DeleteGroup);
            SelectGroupCommand = new RelayCommand(SelectGroup);
            AddAccountCommand = new RelayCommand(AddAccount, _ => SelectedGroup != null);
            EditAccountCommand = new RelayCommand(EditAccount);
            DeleteAccountCommand = new RelayCommand(DeleteAccount);
            CopyEmailCommand = new RelayCommand(CopyEmail);
            CopyUsernameCommand = new RelayCommand(CopyUsername);
            CopyPasswordCommand = new RelayCommand(CopyPassword);
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            DismissErrorCommand = new RelayCommand(DismissError);
            ToggleThemeCommand = new RelayCommand(_ => ThemeService.ToggleTheme());

            LoadData();
        }

        private void SelectGroup(object parameter)
        {
            if (parameter is AccountGroup group)
            {
                SelectedGroup = group;
            }
        }

        private void LoadData()
        {
            try
            {
                HasError = false;
                ErrorMessage = "";
                
                var data = _jsonService.LoadData();
                Groups.Clear();
                foreach (var group in data.Groups)
                {
                    Groups.Add(group);
                }

                // Don't auto-select any group - let user choose
                if (Groups.Any())
                {
                    OnPropertyChanged(nameof(IsEmptyStateVisible));
                    OnPropertyChanged(nameof(IsNoGroupsStateVisible));
                }
            }
            catch (System.IO.InvalidDataException ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
                Console.WriteLine($"Data loading error: {ex.Message}");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Unexpected error loading accounts: {ex.Message}";
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private void DismissError(object parameter)
        {
            HasError = false;
            ErrorMessage = "";
        }

        private void SaveData()
        {
            try
            {
                var data = new AccountData { Groups = Groups.ToList() };
                _jsonService.SaveData(data);
                System.Diagnostics.Debug.WriteLine("Data saved successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddGroup(object parameter)
        {
            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForCreate();
                
                var result = await DialogHelper.ShowDialogAsync(dialog);
                
                Console.WriteLine($"Add group dialog result: {result}");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    var newGroup = dialog.ViewModel.CreateGroup();
                    if (newGroup != null)
                    {
                        Groups.Add(newGroup);
                        SelectedGroup = newGroup;
                        SaveData();
                        
                        OnPropertyChanged(nameof(IsNoGroupsStateVisible));
                        OnPropertyChanged(nameof(IsEmptyStateVisible));
                        
                        System.Diagnostics.Debug.WriteLine($"Group added successfully. Total groups: {Groups.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding group: {ex.Message}");
                MessageBox.Show($"Error adding group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditGroup(object parameter)
        {
            if (parameter is not AccountGroup group) return;

            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForEdit(group);
                
                var result = await DialogHelper.ShowDialogAsync(dialog);
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    dialog.ViewModel.ApplyChanges();
                    SaveData();
                    Console.WriteLine("Group updated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing group: {ex.Message}");
                MessageBox.Show($"Error editing group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteGroup(object parameter)
        {
            if (parameter is not AccountGroup group) return;

            try
            {
                string message;
                string title;
                
                if (group.Accounts.Any())
                {
                    message = $"Group '{group.Name}' contains {group.Accounts.Count} account(s).\n\n" +
                             "Deleting this group will permanently remove all accounts in it.\n\n" +
                             "Are you sure you want to continue?";
                    title = "Delete Group with Accounts";
                }
                else
                {
                    message = $"Are you sure you want to delete the group '{group.Name}'?";
                    title = "Delete Group";
                }

                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    group.Accounts.Any() ? MessageBoxImage.Warning : MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Groups.Remove(group);
                    
                    if (SelectedGroup == group)
                    {
                        SelectedGroup = null;
                    }
                    
                    SaveData();
                    Console.WriteLine("Group deleted successfully");
                    
                    // Update UI state
                    OnPropertyChanged(nameof(IsNoGroupsStateVisible));
                    OnPropertyChanged(nameof(IsEmptyStateVisible));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting group: {ex.Message}");
                MessageBox.Show($"Error deleting group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddAccount(object parameter)
        {
            if (SelectedGroup == null) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForCreate();
                
                var result = await DialogHelper.ShowDialogAsync(dialog);
                
                Console.WriteLine($"Add account dialog result: {result}");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    var newAccount = dialog.ViewModel.CreateAccount();
                    if (newAccount != null)
                    {
                        SelectedGroup.Accounts.Add(newAccount);
                        UpdateFilteredAccounts();
                        SaveData();
                        
                        System.Diagnostics.Debug.WriteLine($"Account added successfully. Total accounts: {SelectedGroup.Accounts.Count}");
                        Console.WriteLine($"Account added successfully. Total accounts: {SelectedGroup.Accounts.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding account: {ex.Message}");
                MessageBox.Show($"Error adding account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditAccount(object parameter)
        {
            if (parameter is not Account account) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForEdit(account);
                
                var result = await DialogHelper.ShowDialogAsync(dialog);
                
                Console.WriteLine($"Edit account dialog result: {result}");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    dialog.ViewModel.ApplyChanges();
                    SaveData();
                    Console.WriteLine("Account updated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing account: {ex.Message}");
                MessageBox.Show($"Error editing account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAccount(object parameter)
        {
            if (parameter is Account account && SelectedGroup != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{account.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedGroup.Accounts.Remove(account);
                    UpdateFilteredAccounts();
                    SaveData();
                }
            }
        }

        private void CopyEmail(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Email))
            {
                try
                {
                    Clipboard.SetText(account.Email);
                    Console.WriteLine($"Email copied for account: {account.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying email: {ex.Message}");
                    MessageBox.Show("Failed to copy email to clipboard.", "Copy Error", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CopyUsername(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Username))
            {
                try
                {
                    Clipboard.SetText(account.Username);
                    Console.WriteLine($"Username copied for account: {account.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying username: {ex.Message}");
                    MessageBox.Show("Failed to copy username to clipboard.", "Copy Error", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void CopyPassword(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Password))
            {
                try
                {
                    Clipboard.SetText(account.Password);
                    Console.WriteLine($"Password copied for account: {account.Name}");
                    // You could show a snackbar notification here in the future
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying password: {ex.Message}");
                    MessageBox.Show("Failed to copy password to clipboard.", "Copy Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UpdateFilteredAccounts()
        {
            FilteredAccounts.Clear();
            
            if (SelectedGroup == null) return;

            var accounts = SelectedGroup.Accounts.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                accounts = accounts.Where(a => 
                    (a.Name?.ToLower().Contains(search) ?? false) ||
                    (a.Username?.ToLower().Contains(search) ?? false) ||
                    (a.Email?.ToLower().Contains(search) ?? false));
            }

            foreach (var account in accounts)
            {
                FilteredAccounts.Add(account);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Keep these classes for backwards compatibility with existing XAML bindings
    // They are now simplified and delegate to the new Views.cs classes
    
    public class EditAccountViewModel : INotifyPropertyChanged
    {
        private Account _account = new();
        
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddGroupViewModel : INotifyPropertyChanged
    {
        private string _groupName = "";

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}