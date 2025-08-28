using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Managers;
using AccountManager.Models;
using AccountManager.Utilities.Helpers;
using AccountManager.Views.Dialogs;

namespace AccountManager.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        private string _searchText = "";
        private string _globalSearchText = "";
        private ObservableCollection<Account> _filteredAccounts = new();
        private ObservableCollection<Account> _globalSearchResults = new();
        private string _errorMessage = "";
        private bool _hasError = false;
        private bool _isListViewMode = false;

        // Current selection state (managed by SidebarViewModel)
        private AccountGroup _currentGroup;
        private SystemGroup _currentSystemGroup;

        // New architecture dependencies
        private readonly ApplicationStateManager _stateManager;
        private readonly ThemeManager _themeManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly IDialogManager _dialogManager;
        private readonly INotificationService _notificationService;

        public ObservableCollection<AccountGroup> Groups { get; set; } = new();
        public SidebarViewModel SidebarViewModel { get; private set; }
        public ThemeManager ThemeService => _themeManager;
        public bool EnableLocalSearch => _configurationManager?.EnableLocalSearch ?? true;
        public bool CensorAccountData => _configurationManager?.CensorAccountData ?? false;
        public bool CensorPassword => _configurationManager?.CensorPassword ?? true;

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

        public bool IsEmptyStateVisible =>
            _currentSystemGroup == null && _currentGroup == null && Groups.Any() && !IsGlobalSearchActive;

        public bool IsNoGroupsStateVisible => !Groups.Any();
        public bool IsAccountsPanelVisible => _currentSystemGroup != null || _currentGroup != null || IsGlobalSearchActive;
        public bool IsLocalSearchVisible => (_currentSystemGroup != null || _currentGroup != null) && !IsGlobalSearchActive;

        // Special view states
        public bool IsViewingTrash => _currentSystemGroup?.Id == "trash";
        public bool IsViewingArchive => _currentSystemGroup?.Id == "archive";
        public bool IsViewingFavorites => _currentSystemGroup?.Id == "favorites";
        public bool IsViewingAll => _currentSystemGroup?.Id == "all";
        public bool IsViewingActiveAccounts => IsViewingAll || IsViewingFavorites || _currentGroup != null;
        public bool CanAddAccount => _currentGroup != null && !IsViewingSystemGroup;
        public bool IsViewingSystemGroup => _currentSystemGroup != null;

        // View Mode Properties
        public bool IsListViewMode
        {
            get => _isListViewMode;
            set => SetProperty(ref _isListViewMode, value);
        }

        // Dynamic UI Content Properties
        public string BreadcrumbText
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"Search Results for \"{GlobalSearchText}\"";
                if (_currentSystemGroup != null)
                    return _currentSystemGroup.Name;
                return _currentGroup != null ? $"Vaults > {_currentGroup.Name}" : "Vaults";
            }
        }

        public string HeaderTitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "Search Results";
                if (_currentSystemGroup != null)
                    return _currentSystemGroup.Name;
                return _currentGroup?.Name ?? "Select Group";
            }
        }

        public string HeaderSubtitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"{GlobalSearchResults.Count} accounts found";
                if (_currentSystemGroup != null)
                    return $"{_currentSystemGroup.Count} accounts";
                return _currentGroup != null ? $"{_currentGroup.Accounts.Count(a => a.IsActive)} accounts" : "0 accounts";
            }
        }

        public string HeaderIcon
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "Magnify";
                if (_currentSystemGroup != null)
                    return _currentSystemGroup.Icon;
                return "Account";
            }
        }

        public string EmptyStateIcon
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "AccountSearch";
                if (_currentSystemGroup?.Id == "favorites")
                    return "StarOutline";
                if (_currentSystemGroup?.Id == "archive")
                    return "Archive";
                if (_currentSystemGroup?.Id == "trash")
                    return "Delete";
                return "AccountOff";
            }
        }

        public string EmptyStateTitle
        {
            get
            {
                if (IsGlobalSearchActive)
                    return "No accounts found";
                if (_currentSystemGroup?.Id == "all")
                    return "No accounts yet";
                if (_currentSystemGroup?.Id == "favorites")
                    return "No favorites yet";
                if (_currentSystemGroup?.Id == "archive")
                    return "No archived accounts";
                if (_currentSystemGroup?.Id == "trash")
                    return "Trash is empty";
                
                // If we have groups but no accounts anywhere, show "No accounts yet" even when no group selected
                if (_currentGroup == null && _currentSystemGroup == null && Groups.Any())
                {
                    var totalAccounts = Groups.SelectMany(g => g.Accounts).Count(a => a.IsActive);
                    return totalAccounts == 0 ? "No accounts yet" : "Select a group";
                }
                
                return _currentGroup != null ? "No accounts yet" : "Select a group";
            }
        }

        public string EmptyStateMessage
        {
            get
            {
                if (IsGlobalSearchActive)
                    return $"No accounts match \"{GlobalSearchText}\"";
                if (_currentSystemGroup?.Id == "all")
                    return "Create your first account to get started";
                if (_currentSystemGroup?.Id == "favorites")
                    return "Mark accounts as favorites by clicking the star icon";
                if (_currentSystemGroup?.Id == "archive")
                    return "Archived accounts will appear here";
                if (_currentSystemGroup?.Id == "trash")
                    return "Deleted accounts will appear here";
                
                // If we have groups but no accounts anywhere, show appropriate message even when no group selected
                if (_currentGroup == null && _currentSystemGroup == null && Groups.Any())
                {
                    var totalAccounts = Groups.SelectMany(g => g.Accounts).Count(a => a.IsActive);
                    return totalAccounts == 0 ? "Create your first account to get started" : "Choose a group from the sidebar";
                }
                
                return _currentGroup != null
                    ? "Click the + button to create your first account"
                    : "Choose a group from the sidebar";
            }
        }

        // Commands
        public ICommand AddAccountCommand { get; private set; }
        public ICommand EditAccountCommand { get; private set; }
        public ICommand DeleteAccountCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public ICommand ArchiveAccountCommand { get; private set; }
        public ICommand RestoreAccountCommand { get; private set; }
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
            // Initialize dependencies from the service container
            var serviceContainer = ServiceContainer.Instance;
            _stateManager = serviceContainer.StateManager;
            _themeManager = serviceContainer.ThemeManager;
            _configurationManager = serviceContainer.ConfigurationManager;
            _dialogManager = serviceContainer.DialogManager;
            _notificationService = serviceContainer.NotificationService;

            SidebarViewModel = new SidebarViewModel();

            InitializeCommands();
            LoadDataAsync(); // Load data FIRST before subscribing to events
            SubscribeToSidebarEvents();
            SubscribeToSettingsEvents();
        }

        private void InitializeCommands()
        {
            AddAccountCommand = new RelayCommand(AddAccount, _ => CanAddAccount); // Updated condition
            EditAccountCommand = new RelayCommand(EditAccount);
            DeleteAccountCommand = new RelayCommand(DeleteAccount);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ArchiveAccountCommand = new RelayCommand(ArchiveAccount);
            RestoreAccountCommand = new RelayCommand(RestoreAccount);
            CopyEmailCommand = new RelayCommand(CopyEmail);
            CopyUsernameCommand = new RelayCommand(CopyUsername);
            CopyPasswordCommand = new RelayCommand(CopyPassword);
            CopyWebsiteCommand = new RelayCommand(CopyWebsite);
            OpenWebsiteCommand = new RelayCommand(OpenWebsite);
            ClearSearchCommand = new RelayCommand(_ => SearchText = "");
            ClearGlobalSearchCommand = new RelayCommand(_ => GlobalSearchText = "");
            DismissErrorCommand = new RelayCommand(DismissError);
            ToggleThemeCommand = new RelayCommand(async _ => await _themeManager.ToggleThemeAsync());
        }

        private void SubscribeToSidebarEvents()
        {
            SidebarViewModel.GroupSelectionChanged += OnGroupSelectionChanged;
            SidebarViewModel.SystemGroupSelectionChanged += OnSystemGroupSelectionChanged;
            SidebarViewModel.DataChanged += OnSidebarDataChanged;
        }

        private void SubscribeToSettingsEvents()
        {
            _configurationManager.TrashSettingChanged += OnTrashSettingChanged;
            _configurationManager.ArchiveSettingChanged += OnArchiveSettingChanged;
            _configurationManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_configurationManager.EnableLocalSearch))
                {
                    OnPropertyChanged(nameof(EnableLocalSearch));
                }
                else if (e.PropertyName == nameof(_configurationManager.CensorAccountData))
                {
                    OnPropertyChanged(nameof(CensorAccountData));
                }
                else if (e.PropertyName == nameof(_configurationManager.CensorPassword))
                {
                    OnPropertyChanged(nameof(CensorPassword));
                }
            };
        }

        private void OnGroupSelectionChanged(object sender, GroupSelectionEventArgs e)
        {
            GlobalSearchText = "";
            _currentGroup = e.SelectedGroup;
            _currentSystemGroup = null;
            
            RefreshUIAfterSelection();
            UpdateFilteredAccounts();
            UpdateDisplayedAccounts();
        }

        private void OnSystemGroupSelectionChanged(object sender, SystemGroupSelectionEventArgs e)
        {
            GlobalSearchText = "";
            _currentSystemGroup = e.SelectedSystemGroup;
            _currentGroup = null;
            
            RefreshUIAfterSelection();
            UpdateFilteredAccounts();
            UpdateDisplayedAccounts();
        }

        private void OnSidebarDataChanged(object sender, EventArgs e)
        {
            // Reload groups when sidebar data changes
            Groups.Clear();
            foreach (var group in SidebarViewModel.AllGroups)
            {
                Groups.Add(group);
            }

            UpdateGlobalSearchResults();
            UpdateFilteredAccounts(); // This ensures filtered accounts reflect group changes
            UpdateDisplayedAccounts();
            RefreshUIAfterSelection();
        }

        private void OnTrashSettingChanged(bool newValue)
        {
            SidebarViewModel.OnTrashSettingChanged(newValue);
        }

        private void OnArchiveSettingChanged(bool newValue)
        {
            SidebarViewModel.OnArchiveSettingChanged(newValue);
        }

        private void RefreshUIAfterSelection()
        {
            OnPropertiesChanged(
                nameof(IsEmptyStateVisible),
                nameof(IsNoGroupsStateVisible),
                nameof(IsAccountsPanelVisible),
                nameof(IsLocalSearchVisible),
                nameof(BreadcrumbText),
                nameof(HeaderTitle),
                nameof(HeaderSubtitle),
                nameof(HeaderIcon),
                nameof(EmptyStateIcon),
                nameof(EmptyStateTitle),
                nameof(EmptyStateMessage),
                nameof(IsViewingTrash),
                nameof(IsViewingArchive),
                nameof(IsViewingFavorites),
                nameof(IsViewingAll),
                nameof(IsViewingActiveAccounts),
                nameof(CanAddAccount),           
                nameof(IsViewingSystemGroup)
            );
        }

        private async void LoadDataAsync()
        {
            try
            {
                HasError = false;
                ErrorMessage = "";

                var data = await _stateManager.GetCurrentDataAsync();
                Groups.Clear();
                
                if (data?.Groups != null)
                {
                    foreach (var group in data.Groups)
                    {
                        Groups.Add(group);
                    }
                }

                // Load groups into sidebar
                SidebarViewModel.LoadGroups(Groups);

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

        private void NotifyDataChanged()
        {
            SidebarViewModel.RefreshCounts();
            UpdateGlobalSearchResults();
            UpdateDisplayedAccounts();
        }

        // Account Operations
        private async void AddAccount(object parameter)
        {
            if (_currentGroup == null) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForCreate();

                var result = await _dialogManager.ShowDialogAsync(dialog);

                if (result == true)
                {
                    // Use the new validation method that shows notifications
                    if (dialog.ViewModel?.ValidateAndNotify() == true)
                    {
                        var newAccount = dialog.ViewModel.CreateAccount();
                        if (newAccount != null)
                        {
                            _currentGroup.Accounts.Add(newAccount);
                            SaveData();
                            NotifyDataChanged();
                            UpdateFilteredAccounts();
                            OnPropertyChanged(nameof(HeaderSubtitle));
                            
                            // Show success notification
                            _notificationService.ShowSuccess($"Account '{newAccount.Name}' has been created successfully.", "Account Created");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error adding account: {ex.Message}", "Error");
            }
        }

        private async void EditAccount(object parameter)
        {
            if (parameter is not Account account) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForEdit(account);

                var result = await _dialogManager.ShowDialogAsync(dialog);

                if (result == true)
                {
                    // Use the new validation method that shows notifications
                    if (dialog.ViewModel?.ValidateAndNotify() == true)
                    {
                        dialog.ViewModel.ApplyChanges();
                        SaveData();
                        NotifyDataChanged();
                        UpdateFilteredAccounts();
                        
                        // Show success notification
                        _notificationService.ShowSuccess($"Account '{account.Name}' has been updated successfully.", "Account Updated");
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error editing account: {ex.Message}", "Error");
            }
        }

        private async void DeleteAccount(object parameter)
        {
            if (parameter is not Account account) return;

            var containingGroup = Groups.FirstOrDefault(g => g.Accounts.Contains(account));
            if (containingGroup == null) return;

            try
            {
                // If we're viewing trash, or trash is disabled, permanently delete
                if (IsViewingTrash || !_configurationManager.EnableTrash)
                {
                    // Permanent deletion
                    if (_configurationManager.ConfirmAccountDelete)
                    {
                        var confirmDialog = new ConfirmationDialog();
                        confirmDialog.SetupForAccountDelete(account);

                        var result = await _dialogManager.ShowDialogAsync(confirmDialog);

                        if (result != true || !confirmDialog.ViewModel.Result)
                            return;
                    }

                    if (IsViewingTrash)
                    {
                        // Remove from trash and delete permanently
                        containingGroup.Accounts.Remove(account);
                        SidebarViewModel.TrashedAccounts.Remove(account);
                        SaveData();
                        SidebarViewModel.UpdateCounts();
                        _notificationService.ShowSuccess($"Account '{account.Name}' has been permanently deleted.", "Account Deleted");
                    }
                    else
                    {
                        // Direct permanent deletion (trash is disabled)
                        containingGroup.Accounts.Remove(account);
                        SaveData();
                        _notificationService.ShowSuccess($"Account '{account.Name}' has been deleted.", "Account Deleted");
                    }
                }
                else
                {
                    // Move to trash
                    if (_configurationManager.ConfirmAccountDelete)
                    {
                        var confirmDialog = new ConfirmationDialog();
                        confirmDialog.SetupForAccountTrash(account);

                        var result = await _dialogManager.ShowDialogAsync(confirmDialog);

                        if (result != true || !confirmDialog.ViewModel.Result)
                            return;
                    }

                    SidebarViewModel.MoveAccountToTrash(account);
                    _notificationService.ShowSuccess($"Account '{account.Name}' has been moved to trash.", "Account Trashed");
                }

                NotifyDataChanged();
                UpdateFilteredAccounts();
                OnPropertyChanged(nameof(HeaderSubtitle));
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error deleting account: {ex.Message}", "Error");
            }
        }

        private void ToggleFavorite(object parameter)
        {
            if (parameter is Account account)
            {
                account.IsFavorite = !account.IsFavorite;
                SaveData();
                NotifyDataChanged();

                // Show notification based on favorite status
                var status = account.IsFavorite ? "added to" : "removed from";
                _notificationService.ShowSuccess($"Account '{account.Name}' has been {status} favorites.", "Favorites Updated");

                // Refresh filtered accounts if viewing favorites
                if (_currentSystemGroup?.Id == "favorites")
                {
                    UpdateFilteredAccounts();
                }

                OnPropertyChanged(nameof(HeaderSubtitle));
            }
        }

        private async void ArchiveAccount(object parameter)
        {
            if (parameter is not Account account) return;

            if (!_configurationManager.EnableArchive)
            {
                MessageBox.Show("Archive functionality is disabled in settings.", "Archive Disabled", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (_configurationManager.ConfirmArchiveAccount)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.SetupForAccountArchive(account);

                    var result = await _dialogManager.ShowDialogAsync(confirmDialog);

                    if (result != true || !confirmDialog.ViewModel.Result)
                        return;
                }

                SidebarViewModel.MoveAccountToArchive(account);
                NotifyDataChanged();
                UpdateFilteredAccounts();
                OnPropertyChanged(nameof(HeaderSubtitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error archiving account: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RestoreAccount(object parameter)
        {
            if (parameter is not Account account) return;

            try
            {
                if (account.IsArchived)
                {
                    SidebarViewModel.RestoreArchivedAccountToGroup(account);
                }
                else
                {
                    SidebarViewModel.RestoreAccount(account);
                }
                
                NotifyDataChanged();
                UpdateFilteredAccounts();
                OnPropertyChanged(nameof(HeaderSubtitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring account: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void SaveData()
        {
            try
            {
                // Use StateManager to update Groups and save all data
                await _stateManager.UpdateGroupsAsync(Groups.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Save Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                catch (Exception)
                {
                    MessageBox.Show("Failed to open website.", "Open Website Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Search Operations
        private void UpdateFilteredAccounts()
        {
            ObservableCollection<Account> sourceAccounts = new();

            if (_currentSystemGroup?.Id == "all")
            {
                // Get all active accounts from all groups (not trashed or archived)
                var allActiveAccounts = Groups?.SelectMany(g => g?.Accounts ?? new ObservableCollection<Account>())
                                              .Where(a => a?.IsActive == true)
                                              .ToArray() ?? new Account[0];
                sourceAccounts = new ObservableCollection<Account>(allActiveAccounts);
            }
            else if (_currentSystemGroup?.Id == "favorites")
            {
                // Get all favorite accounts that are active
                sourceAccounts = new ObservableCollection<Account>(
                    Groups.SelectMany(g => g.Accounts).Where(a => a.IsFavorite && a.IsActive)
                );
            }
            else if (_currentSystemGroup?.Id == "archive")
            {
                // Get all archived accounts
                sourceAccounts = new ObservableCollection<Account>(SidebarViewModel.ArchivedAccounts);
            }
            else if (_currentSystemGroup?.Id == "trash")
            {
                // Get all trashed accounts
                sourceAccounts = new ObservableCollection<Account>(SidebarViewModel.TrashedAccounts);
            }
            else if (_currentGroup != null)
            {
                // Get active accounts from selected group
                sourceAccounts = new ObservableCollection<Account>(
                    _currentGroup.Accounts.Where(a => a.IsActive)
                );
            }
            else
            {
                return;
            }

            var accounts = sourceAccounts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                accounts = accounts.Where(a =>
                    (a.Name?.ToLower().Contains(search) ?? false) ||
                    (a.Username?.ToLower().Contains(search) ?? false) ||
                    (a.Email?.ToLower().Contains(search) ?? false) ||
                    (a.Website?.ToLower().Contains(search) ?? false));
            }

            // Clear and repopulate FilteredAccounts to ensure proper UI refresh
            FilteredAccounts.Clear();
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
            var allAccounts = Groups.SelectMany(g => g.Accounts).Where(a => a.IsActive);

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

        public void Dispose()
        {
            _configurationManager.TrashSettingChanged -= OnTrashSettingChanged;
            _configurationManager.ArchiveSettingChanged -= OnArchiveSettingChanged;
            
            if (SidebarViewModel != null)
            {
                SidebarViewModel.GroupSelectionChanged -= OnGroupSelectionChanged;
                SidebarViewModel.SystemGroupSelectionChanged -= OnSystemGroupSelectionChanged;
                SidebarViewModel.DataChanged -= OnSidebarDataChanged;
            }
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