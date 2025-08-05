using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AccountManager.Commands;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Views.Dialogs;

namespace AccountManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly JsonService _jsonService;
        private AccountGroup _selectedGroup;
        private string _searchText = "";
        private string _globalSearchText = "";
        private ObservableCollection<Account> _filteredAccounts = new();
        private ObservableCollection<Account> _globalSearchResults = new();
        private string _errorMessage = "";
        private bool _hasError = false;

        public ObservableCollection<AccountGroup> Groups { get; set; } = new();
        public ThemeService ThemeService => ThemeService.Instance;

        public AccountGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    OnPropertiesChanged(
                        nameof(IsEmptyStateVisible),
                        nameof(IsNoGroupsStateVisible),
                        nameof(IsAccountsPanelVisible),
                        nameof(IsLocalSearchVisible),
                        nameof(BreadcrumbText),
                        nameof(HeaderTitle),
                        nameof(HeaderSubtitle),
                        nameof(HeaderIcon)
                    );
                    UpdateFilteredAccounts();
                    UpdateDisplayedAccounts();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    UpdateFilteredAccounts();
                    UpdateDisplayedAccounts();
                }
            }
        }

        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (SetProperty(ref _globalSearchText, value))
                {
                    OnPropertiesChanged(
                        nameof(IsGlobalSearchActive),
                        nameof(BreadcrumbText),
                        nameof(HeaderTitle),
                        nameof(HeaderSubtitle),
                        nameof(HeaderIcon),
                        nameof(EmptyStateIcon),
                        nameof(EmptyStateTitle),
                        nameof(EmptyStateMessage)
                    );
                    UpdateGlobalSearchResults();
                    UpdateDisplayedAccounts();
                }
            }
        }

        public ObservableCollection<Account> FilteredAccounts
        {
            get => _filteredAccounts;
            set => SetProperty(ref _filteredAccounts, value);
        }

        public ObservableCollection<Account> GlobalSearchResults
        {
            get => _globalSearchResults;
            set => SetProperty(ref _globalSearchResults, value);
        }

        public ObservableCollection<Account> DisplayedAccounts => 
            IsGlobalSearchActive ? GlobalSearchResults : FilteredAccounts;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        // UI State Properties
        public bool IsGlobalSearchActive => !string.IsNullOrWhiteSpace(GlobalSearchText);
        public bool IsEmptyStateVisible => SelectedGroup == null && Groups.Any() && !IsGlobalSearchActive;
        public bool IsNoGroupsStateVisible => !Groups.Any();
        public bool IsAccountsPanelVisible => SelectedGroup != null || IsGlobalSearchActive;
        public bool IsLocalSearchVisible => SelectedGroup != null && !IsGlobalSearchActive;

        // Dynamic UI Content Properties
        public string BreadcrumbText
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"Search Results for \"{GlobalSearchText}\"";
                return SelectedGroup != null ? $"Groups > {SelectedGroup.Name}" : "Groups";
            }
        }

        public string HeaderTitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "Search Results";
                return SelectedGroup?.Name ?? "Select Group";
            }
        }

        public string HeaderSubtitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"{GlobalSearchResults.Count} accounts found";
                return SelectedGroup != null ? $"{SelectedGroup.Accounts.Count} accounts" : "0 accounts";
            }
        }

        public string HeaderIcon => IsGlobalSearchActive ? "Magnify" : "Account";

        public string EmptyStateIcon => IsGlobalSearchActive ? "AccountSearch" : "AccountOff";

        public string EmptyStateTitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "No accounts found";
                return SelectedGroup != null ? "No accounts yet" : "Select a group";
            }
        }

        public string EmptyStateMessage
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"No accounts match \"{GlobalSearchText}\"";
                return SelectedGroup != null ? "Click the + button to create your first account" : "Choose a group from the sidebar";
            }
        }

        // Commands
        public ICommand AddGroupCommand { get; private set; }
        public ICommand EditGroupCommand { get; private set; }
        public ICommand DeleteGroupCommand { get; private set; }
        public ICommand SelectGroupCommand { get; private set; }
        public ICommand AddAccountCommand { get; private set; }
        public ICommand EditAccountCommand { get; private set; }
        public ICommand DeleteAccountCommand { get; private set; }
        public ICommand CopyEmailCommand { get; private set; }
        public ICommand CopyUsernameCommand { get; private set; }
        public ICommand CopyPasswordCommand { get; private set; }
        public ICommand CopyWebsiteCommand { get; private set; }
        public ICommand OpenWebsiteCommand { get; private set; }
        public ICommand ClearSearchCommand { get; private set; }
        public ICommand ClearGlobalSearchCommand { get; private set; }
        public ICommand DismissErrorCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }

        public MainViewModel()
        {
            _jsonService = new JsonService();
            
            InitializeCommands();
            LoadData();
        }

        private void InitializeCommands()
        {
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
            CopyWebsiteCommand = new RelayCommand(CopyWebsite);
            OpenWebsiteCommand = new RelayCommand(OpenWebsite);
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            ClearGlobalSearchCommand = new RelayCommand(_ => GlobalSearchText = "");
            DismissErrorCommand = new RelayCommand(DismissError);
            ToggleThemeCommand = new RelayCommand(_ => ThemeService.ToggleTheme());
        }

        private void SelectGroup(object parameter)
        {
            if (parameter is AccountGroup group)
            {
                GlobalSearchText = "";
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

                if (Groups.Any())
                {
                    OnPropertiesChanged(nameof(IsEmptyStateVisible), nameof(IsNoGroupsStateVisible));
                }
            }
            catch (System.IO.InvalidDataException ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Unexpected error loading accounts: {ex.Message}";
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Group Operations
        private async void AddGroup(object parameter)
        {
            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForCreate();
                
                var result = await DialogService.ShowDialogAsync(dialog);
                
                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    var newGroup = dialog.ViewModel.CreateGroup();
                    if (newGroup != null)
                    {
                        Groups.Add(newGroup);
                        SelectedGroup = newGroup;
                        SaveData();
                        
                        OnPropertiesChanged(nameof(IsNoGroupsStateVisible), nameof(IsEmptyStateVisible));
                    }
                }
            }
            catch (Exception ex)
            {
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
                
                var result = await DialogService.ShowDialogAsync(dialog);
                
                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    dialog.ViewModel.ApplyChanges();
                    SaveData();
                    
                    OnPropertiesChanged(nameof(BreadcrumbText), nameof(HeaderTitle));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteGroup(object parameter)
        {
            if (parameter is not AccountGroup group) return;

            try
            {
                if (SettingsService.Instance.ConfirmGroupDelete)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.SetupForGroupDelete(group);
                    
                    var result = await DialogService.ShowDialogAsync(confirmDialog);
                    
                    if (confirmDialog.DialogResult != true)
                        return;
                }

                Groups.Remove(group);
                
                if (SelectedGroup == group)
                    SelectedGroup = null;
                
                SaveData();
                
                OnPropertiesChanged(nameof(IsNoGroupsStateVisible), nameof(IsEmptyStateVisible));
                UpdateGlobalSearchResults();
                UpdateDisplayedAccounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Account Operations
        private async void AddAccount(object parameter)
        {
            if (SelectedGroup == null) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForCreate();
                
                var result = await DialogService.ShowDialogAsync(dialog);
                
                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    var newAccount = dialog.ViewModel.CreateAccount();
                    if (newAccount != null)
                    {
                        SelectedGroup.Accounts.Add(newAccount);
                        UpdateFilteredAccounts();
                        UpdateGlobalSearchResults();
                        UpdateDisplayedAccounts();
                        SaveData();
                        
                        OnPropertyChanged(nameof(HeaderSubtitle));
                    }
                }
            }
            catch (Exception ex)
            {
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
                
                var result = await DialogService.ShowDialogAsync(dialog);
                
                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    dialog.ViewModel.ApplyChanges();
                    SaveData();
                    
                    UpdateFilteredAccounts();
                    UpdateGlobalSearchResults();
                    UpdateDisplayedAccounts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteAccount(object parameter)
        {
            if (parameter is not Account account) return;

            var containingGroup = Groups.FirstOrDefault(g => g.Accounts.Contains(account));
            if (containingGroup == null) return;

            try
            {
                if (SettingsService.Instance.ConfirmAccountDelete)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.SetupForAccountDelete(account);
                    
                    var result = await DialogService.ShowDialogAsync(confirmDialog);
                    
                    if (result != true || !confirmDialog.ViewModel.Result)
                        return;
                }

                containingGroup.Accounts.Remove(account);
                UpdateFilteredAccounts();
                UpdateGlobalSearchResults();
                UpdateDisplayedAccounts();
                SaveData();
                
                OnPropertyChanged(nameof(HeaderSubtitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clipboard Operations
        private void CopyEmail(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Email))
            {
                ClipboardHelper.SetText(account.Email);
            }
        }

        private void CopyUsername(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Username))
            {
                ClipboardHelper.SetText(account.Username);
            }
        }

        private void CopyPassword(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Password))
            {
                ClipboardHelper.SetText(account.Password);
            }
        }

        private void CopyWebsite(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrEmpty(account.Website))
            {
                ClipboardHelper.SetText(account.Website);
            }
        }

        private void OpenWebsite(object parameter)
        {
            if (parameter is Account account && !string.IsNullOrWhiteSpace(account.Website))
            {
                try
                {
                    var website = account.Website;
                    if (!website.StartsWith("http://") && !website.StartsWith("https://"))
                    {
                        website = "https://" + website;
                    }

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = website,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open website.", "Open Website Error", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Search Operations
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
                    (a.Email?.ToLower().Contains(search) ?? false) ||
                    (a.Website?.ToLower().Contains(search) ?? false));
            }

            foreach (var account in accounts)
            {
                FilteredAccounts.Add(account);
            }
        }

        private void UpdateGlobalSearchResults()
        {
            GlobalSearchResults.Clear();
            
            if (string.IsNullOrWhiteSpace(GlobalSearchText)) return;

            var search = GlobalSearchText.ToLower();
            var allAccounts = Groups.SelectMany(g => g.Accounts);
            
            var matchingAccounts = allAccounts.Where(a => 
                (a.Name?.ToLower().Contains(search) ?? false) ||
                (a.Username?.ToLower().Contains(search) ?? false) ||
                (a.Email?.ToLower().Contains(search) ?? false) ||
                (a.Website?.ToLower().Contains(search) ?? false));

            foreach (var account in matchingAccounts)
            {
                GlobalSearchResults.Add(account);
            }
        }

        private void UpdateDisplayedAccounts()
        {
            OnPropertiesChanged(
                nameof(DisplayedAccounts),
                nameof(EmptyStateIcon),
                nameof(EmptyStateTitle),
                nameof(EmptyStateMessage)
            );
        }
    }

    // Helper class for clipboard operations
    public static class ClipboardHelper
    {
        public static void SetText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to copy to clipboard.", "Copy Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}