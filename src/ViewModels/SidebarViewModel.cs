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
    public class SidebarViewModel : BaseViewModel
    {
        private SystemGroup _selectedSystemGroup;
        private AccountGroup _selectedGroup;

        // New architecture dependencies
        private readonly ApplicationStateManager _stateManager;
        private readonly ThemeManager _themeManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly IDialogManager _dialogManager;
        private readonly INotificationService _notificationService;

        // Events for communication with MainViewModel
        public event EventHandler<GroupSelectionEventArgs> GroupSelectionChanged;
        public event EventHandler<SystemGroupSelectionEventArgs> SystemGroupSelectionChanged;
        public event EventHandler DataChanged;

        public ObservableCollection<SystemGroup> TopSystemGroups { get; set; } = new();
        public ObservableCollection<SystemGroup> BottomSystemGroups { get; set; } = new();
        public ObservableCollection<SystemGroup> SystemGroups { get; set; } = new();
        public ObservableCollection<AccountGroup> RegularGroups { get; set; } = new();
        public ObservableCollection<AccountGroup> AllGroups { get; set; } = new();

        public ThemeManager ThemeService => _themeManager;

        // Special collections for trash and archive
        public ObservableCollection<Account> TrashedAccounts { get; set; } = new();
        public ObservableCollection<Account> ArchivedAccounts { get; set; } = new();

        public SystemGroup SelectedSystemGroup
        {
            get => _selectedSystemGroup;
            set
            {
                if (SetProperty(ref _selectedSystemGroup, value))
                {
                    _selectedGroup = null; // Clear regular group selection
                    UpdateSystemGroupSelection();
                    SystemGroupSelectionChanged?.Invoke(this, new SystemGroupSelectionEventArgs(value));
                    OnPropertyChanged(nameof(SelectedGroup));
                }
            }
        }

        public AccountGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    _selectedSystemGroup = null; // Clear system group selection
                    UpdateSystemGroupSelection();
                    GroupSelectionChanged?.Invoke(this, new GroupSelectionEventArgs(value));
                    OnPropertyChanged(nameof(SelectedSystemGroup));
                }
            }
        }

        public int FavoritesCount => AllGroups.SelectMany(g => g.Accounts).Count(a => a.IsFavorite && !a.IsArchived && !a.IsTrashed);
        public int TotalAccountsCount => AllGroups.SelectMany(g => g.Accounts).Count(a => !a.IsArchived && !a.IsTrashed);
        public int TrashedAccountsCount => TrashedAccounts.Count;
        public int ArchivedAccountsCount => ArchivedAccounts.Count;

        // Commands
        public ICommand AddGroupCommand { get; private set; }
        public ICommand EditGroupCommand { get; private set; }
        public ICommand DeleteGroupCommand { get; private set; }
        public ICommand SelectGroupCommand { get; private set; }
        public ICommand SelectSystemGroupCommand { get; private set; }
        public ICommand EmptyTrashCommand { get; private set; }
        public ICommand RestoreFromTrashCommand { get; private set; }

        public SidebarViewModel()
        {
            // Initialize dependencies from the service container
            var serviceContainer = ServiceContainer.Instance;
            _stateManager = serviceContainer.StateManager;
            _themeManager = serviceContainer.ThemeManager;
            _configurationManager = serviceContainer.ConfigurationManager;
            _dialogManager = serviceContainer.DialogManager;
            _notificationService = serviceContainer.NotificationService;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddGroupCommand = new RelayCommand(AddGroup);
            EditGroupCommand = new RelayCommand(EditGroup);
            DeleteGroupCommand = new RelayCommand(DeleteGroup);
            SelectGroupCommand = new RelayCommand(SelectRegularGroup);
            SelectSystemGroupCommand = new RelayCommand(SelectSystemGroupCommand_Execute);
            EmptyTrashCommand = new RelayCommand(EmptyTrash);
            RestoreFromTrashCommand = new RelayCommand(RestoreFromTrash);
        }

        public void LoadGroups(ObservableCollection<AccountGroup> groups)
        {
            AllGroups.Clear();
            RegularGroups.Clear();
            TrashedAccounts.Clear();
            ArchivedAccounts.Clear();

            if (groups != null)
            {
                foreach (var group in groups)
                {
                    AllGroups.Add(group);
                    RegularGroups.Add(group);

                    if (group?.Accounts != null)
                    {
                        // Separate trashed and archived accounts
                        var trashedAccounts = group.Accounts.Where(a => a.IsTrashed).ToList();
                        var archivedAccounts = group.Accounts.Where(a => a.IsArchived).ToList();
                        
                        foreach (var account in trashedAccounts)
                        {
                            TrashedAccounts.Add(account);
                        }

                        foreach (var account in archivedAccounts)
                        {
                            ArchivedAccounts.Add(account);
                        }
                    }
                }
            }

            InitializeSystemGroups();

            // Only auto-select "All items" if there are no accounts anywhere
            // Otherwise, leave nothing selected to show "Select a group" empty state
            if (SystemGroups.Any())
            {
                var totalActiveAccounts = AllGroups.SelectMany(g => g.Accounts).Count(a => a.IsActive);
                if (totalActiveAccounts == 0)
                {
                    SelectedSystemGroup = SystemGroups.First(sg => sg.Id == "all");
                }
                // If there are accounts, leave nothing selected (SelectedSystemGroup remains null)
            }
        }

        private void InitializeSystemGroups()
        {
            SystemGroups.Clear();
            TopSystemGroups.Clear();
            BottomSystemGroups.Clear();

            // All items - always first
            var allItemsGroup = new SystemGroup
            {
                Id = "all",
                Name = "All items",
                Icon = "ShieldKeyOutline",
                ColorVariant = "#8B5CF6", // Purple
                Count = TotalAccountsCount,
                SortOrder = 0
            };
            TopSystemGroups.Add(allItemsGroup);
            SystemGroups.Add(allItemsGroup);

            // Favorites - second
            if (_configurationManager.ShowFavoritesGroup) // You'll need to add this setting
            {
                var favoritesGroup = new SystemGroup
                {
                    Id = "favorites",
                    Name = "Favorites",
                    Icon = "StarOutline",
                    ColorVariant = "#F59E0B", // Orange
                    Count = FavoritesCount,
                    SortOrder = 1
                };
                TopSystemGroups.Add(favoritesGroup);
                SystemGroups.Add(favoritesGroup);
            }

            // Archive - third (if enabled)
            if (_configurationManager.EnableArchive)
            {
                var archiveGroup = new SystemGroup
                {
                    Id = "archive",
                    Name = "Archive",
                    Icon = "ArchiveOutline",
                    ColorVariant = "#8B5CF6", // Purple
                    Count = ArchivedAccountsCount,
                    SortOrder = 997
                };
                BottomSystemGroups.Add(archiveGroup);
                SystemGroups.Add(archiveGroup);
            }

            // Trash - always last (if enabled)
            if (_configurationManager.EnableTrash)
            {
                var trashGroup = new SystemGroup
                {
                    Id = "trash",
                    Name = "Trash",
                    Icon = "DeleteOutline",
                    ColorVariant = "#6B7280", // Gray
                    Count = TrashedAccountsCount,
                    SortOrder = 999
                };
                BottomSystemGroups.Add(trashGroup);
                SystemGroups.Add(trashGroup);
            }

            // Sort by SortOrder to ensure proper ordering
            var sortedGroups = SystemGroups.OrderBy(sg => sg.SortOrder).ToList();
            SystemGroups.Clear();
            foreach (var group in sortedGroups)
            {
                SystemGroups.Add(group);
            }
        }

        public void UpdateCounts()
        {
            // Update special collections
            RefreshSpecialCollections();

            foreach (var systemGroup in SystemGroups)
            {
                switch (systemGroup.Id)
                {
                    case "all":
                        systemGroup.Count = TotalAccountsCount;
                        break;
                    case "favorites":
                        systemGroup.Count = FavoritesCount;
                        break;
                    case "archive":
                        systemGroup.Count = ArchivedAccountsCount;
                        break;
                    case "trash":
                        systemGroup.Count = TrashedAccountsCount;
                        break;
                }
            }

            OnPropertiesChanged(nameof(FavoritesCount), nameof(TotalAccountsCount), 
                               nameof(TrashedAccountsCount), nameof(ArchivedAccountsCount));
        }

        private void RefreshSpecialCollections()
        {
            TrashedAccounts.Clear();
            ArchivedAccounts.Clear();

            foreach (var group in AllGroups)
            {
                foreach (var account in group.Accounts.Where(a => a.IsTrashed))
                {
                    TrashedAccounts.Add(account);
                }

                foreach (var account in group.Accounts.Where(a => a.IsArchived))
                {
                    ArchivedAccounts.Add(account);
                }
            }
        }

        public void RefreshGroupArrangement()
        {
            InitializeSystemGroups();
            OnPropertiesChanged(nameof(TopSystemGroups), nameof(BottomSystemGroups), nameof(SystemGroups));
        }

        private void UpdateSystemGroupSelection()
        {
            foreach (var systemGroup in SystemGroups)
            {
                systemGroup.IsSelected = systemGroup == SelectedSystemGroup;
            }
        }

        private void SelectRegularGroup(object parameter)
        {
            if (parameter is AccountGroup group)
            {
                SelectedGroup = group;
            }
        }

        private void SelectSystemGroupCommand_Execute(object parameter)
        {
            if (parameter is SystemGroup systemGroup)
            {
                SelectedSystemGroup = systemGroup;
            }
        }

        // Account management methods
        public void MoveAccountToTrash(Account account)
        {
            if (account == null) return;

            // Find the current group containing this account
            var currentGroup = AllGroups.FirstOrDefault(g => g.Accounts.Contains(account));
            if (currentGroup != null)
            {
                account.PreviousGroupId = currentGroup.Id; // Track where it came from
            }

            account.IsTrashed = true;
            account.TrashedDate = DateTime.Now;
            account.IsArchived = false; // Can't be both archived and trashed

            TrashedAccounts.Add(account);
            if (ArchivedAccounts.Contains(account))
                ArchivedAccounts.Remove(account);

            SaveData();
            UpdateCounts();
        }

        public void MoveAccountToArchive(Account account)
        {
            if (account == null) return;

            // Find the current group containing this account
            var currentGroup = AllGroups.FirstOrDefault(g => g.Accounts.Contains(account));
            if (currentGroup != null)
            {
                account.PreviousGroupId = currentGroup.Id; // Track where it came from
            }

            account.IsArchived = true;
            account.ArchivedDate = DateTime.Now;
            account.IsTrashed = false; // Can't be both archived and trashed

            ArchivedAccounts.Add(account);
            if (TrashedAccounts.Contains(account))
                TrashedAccounts.Remove(account);

            SaveData();
            UpdateCounts();
        }

        public void RestoreAccount(Account account)
        {
            if (account == null) return;

            // Try to find the previous group
            AccountGroup targetGroup = null;
            if (!string.IsNullOrEmpty(account.PreviousGroupId))
            {
                targetGroup = AllGroups.FirstOrDefault(g => g.Id == account.PreviousGroupId);
            }

            // If previous group not found, use first group as fallback
            if (targetGroup == null)
            {
                targetGroup = AllGroups.FirstOrDefault();
            }

            if (targetGroup != null)
            {
                account.IsTrashed = false;
                account.IsArchived = false;
                account.TrashedDate = null;
                account.ArchivedDate = null;
                account.PreviousGroupId = null;

                // Add to target group if not already there
                if (!targetGroup.Accounts.Contains(account))
                {
                    targetGroup.Accounts.Add(account);
                }
            }

            TrashedAccounts.Remove(account);
            ArchivedAccounts.Remove(account);

            SaveData();
            UpdateCounts();
        }

        public void RestoreArchivedAccountToGroup(Account account, AccountGroup targetGroup = null)
        {
            if (account == null || !account.IsArchived) return;

            // If no target specified, try to restore to original group
            if (targetGroup == null && !string.IsNullOrEmpty(account.PreviousGroupId))
            {
                targetGroup = AllGroups.FirstOrDefault(g => g.Id == account.PreviousGroupId);
            }

            // If still no target, use first group
            if (targetGroup == null)
            {
                targetGroup = AllGroups.FirstOrDefault();
            }

            if (targetGroup != null)
            {
                account.IsArchived = false;
                account.ArchivedDate = null;
                account.PreviousGroupId = null;

                // Add to target group if not already there
                if (!targetGroup.Accounts.Contains(account))
                {
                    targetGroup.Accounts.Add(account);
                }
            }

            ArchivedAccounts.Remove(account);
            SaveData();
            UpdateCounts();
        }

        private async void EmptyTrash(object parameter)
        {
            if (!TrashedAccounts.Any())
            {
                _notificationService.ShowInfo("Trash is already empty.", "Empty Trash");
                return;
            }

            var confirmDialog = new ConfirmationDialog();
            confirmDialog.SetupForEmptyTrash(TrashedAccounts.Count);

            var result = await _dialogManager.ShowDialogAsync(confirmDialog);

            if (result == true && confirmDialog.ViewModel.Result)
            {
                var deletedCount = TrashedAccounts.Count;
                
                // Permanently delete all trashed accounts
                foreach (var account in TrashedAccounts.ToList())
                {
                    var containingGroup = AllGroups.FirstOrDefault(g => g.Accounts.Contains(account));
                    containingGroup?.Accounts.Remove(account);
                }

                TrashedAccounts.Clear();
                SaveData();
                UpdateCounts();
                DataChanged?.Invoke(this, EventArgs.Empty);
                
                // Show success notification
                _notificationService.ShowSuccess($"{deletedCount} {(deletedCount == 1 ? "account" : "accounts")} permanently deleted", "Trash Emptied");
            }
        }

        private void RestoreFromTrash(object parameter)
        {
            if (parameter is Account account)
            {
                RestoreAccount(account);
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // Settings change handlers
        public void OnTrashSettingChanged(bool newValue)
        {
            if (!newValue && TrashedAccounts.Any())
            {
                // Turning off trash with existing items
                var migrationDialog = new TrashMigrationDialog();
                migrationDialog.SetupDialog(TrashedAccounts.ToList(), AllGroups.ToList());

                var result = _dialogManager.ShowDialogAsync(migrationDialog).Result;

                if (result == true)
                {
                    // Handle migration or deletion based on user choice
                    migrationDialog.ViewModel.ExecuteAction();
                    RefreshSpecialCollections();
                    SaveData();
                    UpdateCounts();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            // Refresh system groups to show/hide trash
            InitializeSystemGroups();
            UpdateCounts();
        }

        public void OnArchiveSettingChanged(bool newValue)
        {
            if (!newValue && ArchivedAccounts.Any())
            {
                // Turning off archive with existing items
                var migrationDialog = new ArchiveMigrationDialog();
                migrationDialog.SetupDialog(ArchivedAccounts.ToList(), AllGroups.ToList());

                var result = _dialogManager.ShowDialogAsync(migrationDialog).Result;

                if (result == true)
                {
                    // Handle migration or deletion based on user choice
                    migrationDialog.ViewModel.ExecuteAction();
                    RefreshSpecialCollections();
                    SaveData();
                    UpdateCounts();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            // Refresh system groups to show/hide archive
            InitializeSystemGroups();
            UpdateCounts();
        }

        private async void AddGroup(object parameter)
        {
            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForCreate();

                var result = await _dialogManager.ShowDialogAsync(dialog);

                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    var newGroup = dialog.ViewModel.CreateGroup();
                    if (newGroup != null)
                    {
                        AllGroups.Add(newGroup);
                        RegularGroups.Add(newGroup);
                        SelectedGroup = newGroup;
                        SaveData();
                        UpdateCounts();
                        DataChanged?.Invoke(this, EventArgs.Empty);
                        
                        // Show success notification
                        _notificationService.ShowSuccess($"Group '{newGroup.Name}' has been created successfully.", "Group Created");
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error adding group: {ex.Message}", "Error");
            }
        }

        private async void EditGroup(object parameter)
        {
            if (parameter is not AccountGroup group) return;

            try
            {
                var dialog = new GroupDialog();
                dialog.SetupForEdit(group);

                var result = await _dialogManager.ShowDialogAsync(dialog);

                if (result == true && dialog.ViewModel?.CanSave == true)
                {
                    dialog.ViewModel.ApplyChanges();
                    SaveData();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                    
                    // Show success notification
                    _notificationService.ShowSuccess($"Group '{group.Name}' has been updated successfully.", "Group Updated");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error editing group: {ex.Message}", "Error");
            }
        }

        private async void DeleteGroup(object parameter)
        {
            if (parameter is not AccountGroup group) return;

            try
            {
                if (_configurationManager.ConfirmGroupDelete)
                {
                    var confirmDialog = new ConfirmationDialog();
                    confirmDialog.SetupForGroupDelete(group);

                    var result = await _dialogManager.ShowDialogAsync(confirmDialog);

                    if (result != true)
                        return;
                }

                AllGroups.Remove(group);
                RegularGroups.Remove(group);

                if (SelectedGroup == group)
                {
                    // Select "All items" when current group is deleted
                    SelectedSystemGroup = SystemGroups.First(sg => sg.Id == "all");
                }

                SaveData();
                UpdateCounts();
                DataChanged?.Invoke(this, EventArgs.Empty);
                
                // Show success notification
                _notificationService.ShowSuccess($"Group '{group.Name}' has been deleted successfully.", "Group Deleted");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error deleting group: {ex.Message}", "Error");
            }
        }

        private async void SaveData()
        {
            try
            {
                // Use StateManager to update Groups and save all data
                await _stateManager.UpdateGroupsAsync(AllGroups.ToList());
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error saving data: {ex.Message}", "Save Error");
            }
        }

        // Method to be called when accounts are modified from MainViewModel
        public void RefreshCounts()
        {
            UpdateCounts();
        }
    }

    // Event argument classes for communication
    public class GroupSelectionEventArgs : EventArgs
    {
        public AccountGroup SelectedGroup { get; }

        public GroupSelectionEventArgs(AccountGroup selectedGroup)
        {
            SelectedGroup = selectedGroup;
        }
    }

    public class SystemGroupSelectionEventArgs : EventArgs
    {
        public SystemGroup SelectedSystemGroup { get; }

        public SystemGroupSelectionEventArgs(SystemGroup selectedSystemGroup)
        {
            SelectedSystemGroup = selectedSystemGroup;
        }
    }

    // System Group model
    public class SystemGroup : BaseViewModel
    {
        private bool _isSelected;
        private int _count;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string ColorVariant { get; set; }
        public int SortOrder { get; set; }

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}