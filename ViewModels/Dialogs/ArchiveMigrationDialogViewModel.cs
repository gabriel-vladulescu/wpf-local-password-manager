using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class ArchiveMigrationDialogViewModel : BaseViewModel
    {
        private bool _isMigrateSelected = true;
        private bool _isDeleteSelected = false;
        private AccountGroup _selectedGroup;
        private string _confirmationText = "";
        private List<Account> _itemsToMigrate;

        public ObservableCollection<AccountGroup> AvailableGroups { get; set; } = new();
        public int ItemCount => _itemsToMigrate?.Count ?? 0;

        public bool IsMigrateSelected
        {
            get => _isMigrateSelected;
            set
            {
                if (SetProperty(ref _isMigrateSelected, value))
                {
                    OnPropertyChanged(nameof(CanProceed));
                }
            }
        }

        public bool IsDeleteSelected
        {
            get => _isDeleteSelected;
            set
            {
                if (SetProperty(ref _isDeleteSelected, value))
                {
                    OnPropertyChanged(nameof(CanProceed));
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
                    OnPropertyChanged(nameof(CanProceed));
                }
            }
        }

        public string ConfirmationText
        {
            get => _confirmationText;
            set
            {
                if (SetProperty(ref _confirmationText, value))
                {
                    OnPropertyChanged(nameof(CanProceed));
                }
            }
        }

        public bool CanProceed
        {
            get
            {
                if (IsMigrateSelected)
                    return SelectedGroup != null;
                
                if (IsDeleteSelected)
                    return string.Equals(ConfirmationText, "DELETE", StringComparison.OrdinalIgnoreCase);
                
                return false;
            }
        }

        public ICommand ProceedCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ArchiveMigrationDialogViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ProceedCommand = new RelayCommand(Proceed, _ => CanProceed);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void SetupDialog(List<Account> itemsToMigrate, List<AccountGroup> availableGroups)
        {
            _itemsToMigrate = itemsToMigrate;
            
            AvailableGroups.Clear();
            foreach (var group in availableGroups)
            {
                AvailableGroups.Add(group);
            }

            // Select first group by default
            if (AvailableGroups.Any())
            {
                SelectedGroup = AvailableGroups.First();
            }

            OnPropertyChanged(nameof(ItemCount));
        }

        public void ExecuteAction()
        {
            if (_itemsToMigrate == null || !_itemsToMigrate.Any())
                return;

            if (IsMigrateSelected && SelectedGroup != null)
            {
                // Migrate items to selected group
                foreach (var item in _itemsToMigrate)
                {
                    item.IsArchived = false;
                    item.ArchivedDate = null;
                    
                    // Add to selected group if not already there
                    if (!SelectedGroup.Accounts.Contains(item))
                    {
                        SelectedGroup.Accounts.Add(item);
                    }
                }
            }
            else if (IsDeleteSelected)
            {
                // Permanently delete items
                foreach (var item in _itemsToMigrate)
                {
                    // Find and remove from all groups
                    var containingGroups = AvailableGroups.Where(g => g.Accounts.Contains(item)).ToList();
                    foreach (var group in containingGroups)
                    {
                        group.Accounts.Remove(item);
                    }
                }
            }
        }

        private void Proceed(object parameter)
        {
            // Dialog result will be handled by the dialog service
        }

        private void Cancel(object parameter)
        {
            // Dialog result will be handled by the dialog service
        }
    }
}