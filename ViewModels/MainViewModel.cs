using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Views.Dialogs;
using AccountManager.ViewModels.Commands;

namespace AccountManager.ViewModels
{
    /// <summary>
    /// Main application view model
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region Private Fields

        private readonly JsonService _jsonService;
        private readonly ValidationService _validationService;
        private AccountGroup _selectedGroup;
        private string _searchText = "";
        private string _globalSearchText = "";
        private ObservableCollection<Account> _filteredAccounts = new();
        private ObservableCollection<Account> _globalSearchResults = new();

        #endregion

        #region Public Properties

        public ObservableCollection<AccountGroup> Groups { get; set; } = new();
        public ThemeService ThemeService => ThemeService.Instance;
        public SettingsService SettingsService => SettingsService.Instance;

        public AccountGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    OnSelectedGroupChanged();
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
                    OnGlobalSearchChanged();
                }
            }
        }

        public ObservableCollection<Account> FilteredAccounts
        {
            get => _filteredAccounts;
            private set => SetProperty(ref _filteredAccounts, value);
        }

        public ObservableCollection<Account> GlobalSearchResults
        {
            get => _globalSearchResults;
            private set => SetProperty(ref _globalSearchResults, value);
        }

        public ObservableCollection<Account> DisplayedAccounts => 
            IsGlobalSearchActive ? GlobalSearchResults : FilteredAccounts;

        #endregion

        #region Computed Properties

        public bool IsGlobalSearchActive => !string.IsNullOrWhiteSpace(GlobalSearchText);
        public bool IsEmptyStateVisible => SelectedGroup == null && Groups.Any() && !IsGlobalSearchActive;
        public bool IsNoGroupsStateVisible => !Groups.Any();
        public bool IsAccountsPanelVisible => SelectedGroup != null || IsGlobalSearchActive;
        public bool IsLocalSearchVisible => SelectedGroup != null && !IsGlobalSearchActive;

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

        #endregion

        #region Commands

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
        public ICommand CopyWebsiteCommand { get; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ClearGlobalSearchCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand RefreshDataCommand { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _jsonService = new JsonService();
            _validationService = ValidationService.Instance;
            
            InitializeCommands();
            
            // Use the async initialization from BaseViewModel
            _ = InitializeAsync();
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            EditGroupCommand = new AsyncRelayCommand<AccountGroup>(EditGroupAsync);
            DeleteGroupCommand = new AsyncRelayCommand<AccountGroup>(DeleteGroupAsync);
            SelectGroupCommand = new RelayCommand<AccountGroup>(SelectGroup);
            AddAccountCommand = new AsyncRelayCommand(AddAccountAsync, () => SelectedGroup != null);
            EditAccountCommand = new AsyncRelayCommand<Account>(EditAccountAsync);
            DeleteAccountCommand = new AsyncRelayCommand<Account>(DeleteAccountAsync);
            CopyEmailCommand = new RelayCommand<Account>(CopyEmail);
            CopyUsernameCommand = new RelayCommand<Account>(CopyUsername);
            CopyPasswordCommand = new RelayCommand<Account>(CopyPassword);
            CopyWebsiteCommand = new RelayCommand<Account>(CopyWebsite);
            OpenWebsiteCommand = new RelayCommand<Account>(OpenWebsite);
            ClearSearchCommand = new RelayCommand(() => SearchText = "");
            ClearGlobalSearchCommand = new RelayCommand(() => GlobalSearchText = "");
            ToggleThemeCommand = new RelayCommand(() => ThemeService.ToggleTheme());
            ShowSettingsCommand = new AsyncRelayCommand(ShowSettingsAsync);
            RefreshDataCommand = new AsyncRelayCommand(() => LoadDataAsync());
        }

        protected override async Task OnInitializeAsync()
        {
            await LoadDataAsync();
        }

        protected override async Task OnRefreshAsync()
        {
            await LoadDataAsync();
        }

        #endregion

        #region Data Operations

        private async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var data = await Task.Run(() => _jsonService.LoadData());
                
                Groups.Clear();
                foreach (var group in data.Groups)
                {
                    Groups.Add(group);
                }

                UpdateUIState();
            }, "Loading accounts...");
        }

        private async Task SaveDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var data = new AccountData { Groups = Groups.ToList() };
                await Task.Run(() => _jsonService.SaveData(data));
            }, "Saving...");
        }

        #endregion

        #region Group Operations

        private async Task AddGroupAsync()
        {
            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForCreate();
                
                var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, "Add Group Error");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    var newGroup = dialog.ViewModel.CreateGroup();
                    if (newGroup != null)
                    {
                        // Validate uniqueness
                        var validation = _validationService.ValidateGroupNameUniqueness(
                            newGroup.Name, Groups);
                        
                        if (!validation.IsValid)
                        {
                            ShowError(validation.FirstError);
                            return;
                        }

                        Groups.Add(newGroup);
                        SelectedGroup = newGroup;
                        await SaveDataAsync();
                        UpdateUIState();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding group: {ex.Message}");
            }
        }

        private async Task EditGroupAsync(AccountGroup group)
        {
            if (group == null) return;

            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForEdit(group);
                
                var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, "Edit Group Error");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    // Validate uniqueness (excluding current group)
                    var validation = _validationService.ValidateGroupNameUniqueness(
                        dialog.ViewModel.GroupName, Groups, group);
                    
                    if (!validation.IsValid)
                    {
                        ShowError(validation.FirstError);
                        return;
                    }

                    dialog.ViewModel.ApplyChanges();
                    await SaveDataAsync();
                    UpdateUIState();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error editing group: {ex.Message}");
            }
        }

        private async Task DeleteGroupAsync(AccountGroup group)
        {
            if (group == null) return;

            try
            {
                // Show confirmation if enabled in settings
                if (SettingsService.ConfirmGroupDelete)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.ViewModel.SetupForGroupDelete(group);
                    
                    var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(confirmDialog, "Delete Group Error");
                    
                    if (result != true || !confirmDialog.ViewModel.Result)
                        return;
                }

                Groups.Remove(group);
                
                if (SelectedGroup == group)
                {
                    SelectedGroup = null;
                }
                
                await SaveDataAsync();
                UpdateUIState();
                UpdateGlobalSearchResults();
                UpdateDisplayedAccounts();
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting group: {ex.Message}");
            }
        }

        private void SelectGroup(AccountGroup group)
        {
            if (group != null)
            {
                GlobalSearchText = ""; // Clear global search when selecting a group
                SelectedGroup = group;
            }
        }

        #endregion

        #region Account Operations

        private async Task AddAccountAsync()
        {
            if (SelectedGroup == null) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForCreate();
                
                var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, "Add Account Error");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    var newAccount = dialog.ViewModel.CreateAccount();
                    if (newAccount != null)
                    {
                        SelectedGroup.Accounts.Add(newAccount);
                        UpdateFilteredAccounts();
                        UpdateGlobalSearchResults();
                        UpdateDisplayedAccounts();
                        await SaveDataAsync();
                        UpdateUIState();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding account: {ex.Message}");
            }
        }

        private async Task EditAccountAsync(Account account)
        {
            if (account == null) return;

            try
            {
                var dialog = new AccountDialog();
                dialog.SetupForEdit(account);
                
                var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, "Edit Account Error");
                
                if (result == true && dialog.ViewModel.CanSave)
                {
                    dialog.ViewModel.ApplyChanges();
                    await SaveDataAsync();
                    
                    // Refresh search results since account data changed
                    UpdateFilteredAccounts();
                    UpdateGlobalSearchResults();
                    UpdateDisplayedAccounts();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error editing account: {ex.Message}");
            }
        }

        private async Task DeleteAccountAsync(Account account)
        {
            if (account == null) return;

            var containingGroup = Groups.FirstOrDefault(g => g.Accounts.Contains(account));
            if (containingGroup == null) return;

            try
            {
                // Show confirmation if enabled
                if (SettingsService.ConfirmAccountDelete)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.ViewModel.SetupForAccountDelete(account);
                    
                    var result = await DialogHelper.ShowDialogWithErrorHandlingAsync(confirmDialog, "Delete Account Error");
                    
                    if (result != true || !confirmDialog.ViewModel.Result)
                        return;
                }

                containingGroup.Accounts.Remove(account);
                UpdateFilteredAccounts();
                UpdateGlobalSearchResults();
                UpdateDisplayedAccounts();
                await SaveDataAsync();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting account: {ex.Message}");
            }
        }

        #endregion

        #region Clipboard Operations

        private void CopyEmail(Account account)
        {
            CopyToClipboard(account?.Email, "email", account?.Name);
        }

        private void CopyUsername(Account account)
        {
            CopyToClipboard(account?.Username, "username", account?.Name);
        }

        private void CopyPassword(Account account)
        {
            CopyToClipboard(account?.Password, "password", account?.Name);
        }

        private void CopyWebsite(Account account)
        {
            CopyToClipboard(account?.Website, "website", account?.Name);
        }

        private void CopyToClipboard(string value, string fieldName, string accountName)
        {
            if (string.IsNullOrEmpty(value)) return;

            try
            {
                Clipboard.SetText(value);
                // Could show a toast notification here using the base class
            }
            catch (Exception ex)
            {
                ShowError($"Failed to copy {fieldName} to clipboard: {ex.Message}");
            }
        }

        private void OpenWebsite(Account account)
        {
            if (account?.Website == null) return;

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
                ShowError($"Failed to open website: {ex.Message}");
            }
        }

        #endregion

        #region Search Operations

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

        #endregion

        #region Settings

        private async Task ShowSettingsAsync()
        {
            try
            {
                var dialog = new SettingsDialog();
                dialog.SetupForView();
                
                await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, "Settings Error");
            }
            catch (Exception ex)
            {
                ShowError($"Error showing settings: {ex.Message}");
            }
        }

        #endregion

        #region UI State Management

        private void OnSelectedGroupChanged()
        {
            UpdateComputedProperties();
            UpdateFilteredAccounts();
            UpdateDisplayedAccounts();
        }

        private void OnGlobalSearchChanged()
        {
            UpdateComputedProperties();
            UpdateGlobalSearchResults();
            UpdateDisplayedAccounts();
        }

        private void UpdateDisplayedAccounts()
        {
            OnPropertyChanged(nameof(DisplayedAccounts));
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(IsGlobalSearchActive));
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(IsNoGroupsStateVisible));
            OnPropertyChanged(nameof(IsAccountsPanelVisible));
            OnPropertyChanged(nameof(IsLocalSearchVisible));
            OnPropertyChanged(nameof(BreadcrumbText));
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(HeaderSubtitle));
            OnPropertyChanged(nameof(HeaderIcon));
            OnPropertyChanged(nameof(EmptyStateIcon));
            OnPropertyChanged(nameof(EmptyStateTitle));
            OnPropertyChanged(nameof(EmptyStateMessage));
        }

        private void UpdateUIState()
        {
            UpdateComputedProperties();
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Protected Overrides

        protected override void OnDisposing()
        {
            // Clean up resources
            Groups?.Clear();
            FilteredAccounts?.Clear();
            GlobalSearchResults?.Clear();
        }

        #endregion
    }
}