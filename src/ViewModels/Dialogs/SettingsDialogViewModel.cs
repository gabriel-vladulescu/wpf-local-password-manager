using System;
using System.Windows.Input;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Managers;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class SettingsDialogViewModel : BaseViewModel
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IPathProvider _pathProvider;
        private readonly ImportExportManager _importExportManager;

        // Privacy & Security Settings
        public bool CensorAccountData
        {
            get => _configurationManager.CensorAccountData;
            set => _configurationManager.CensorAccountData = value;
        }

        public bool CensorPassword
        {
            get => _configurationManager.CensorPassword;
            set => _configurationManager.CensorPassword = value;
        }

        public bool EnableEncryption
        {
            get => _configurationManager.EnableEncryption;
            set => _configurationManager.EnableEncryption = value;
        }

        // Data Management Settings
        public bool EnableTrash
        {
            get => _configurationManager.EnableTrash;
            set => _configurationManager.EnableTrash = value;
        }

        public bool EnableArchive
        {
            get => _configurationManager.EnableArchive;
            set => _configurationManager.EnableArchive = value;
        }

        public bool AutoEmptyTrash
        {
            get => _configurationManager.AutoEmptyTrash;
            set => _configurationManager.AutoEmptyTrash = value;
        }

        public int TrashRetentionDays
        {
            get => _configurationManager.TrashRetentionDays;
            set => _configurationManager.TrashRetentionDays = value;
        }

        // UI Settings
        public bool EnableLocalSearch
        {
            get => _configurationManager.EnableLocalSearch;
            set => _configurationManager.EnableLocalSearch = value;
        }

        // Confirmation Settings
        public bool ConfirmAccountDelete
        {
            get => _configurationManager.ConfirmAccountDelete;
            set => _configurationManager.ConfirmAccountDelete = value;
        }

        public bool ConfirmGroupDelete
        {
            get => _configurationManager.ConfirmGroupDelete;
            set => _configurationManager.ConfirmGroupDelete = value;
        }

        public bool ConfirmArchiveAccount
        {
            get => _configurationManager.ConfirmArchiveAccount;
            set => _configurationManager.ConfirmArchiveAccount = value;
        }

        // File Management Properties
        public string DataFileDisplayPath => _pathProvider?.GetDisplayPath() ?? "Default (AppData)";

        // Computed properties
        public bool ShowTrashRetention => EnableTrash && AutoEmptyTrash;

        // Commands
        public ICommand ResetToDefaultsCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand BrowseDataPathCommand { get; private set; }
        public ICommand ResetDataPathCommand { get; private set; }
        public ICommand ImportDataCommand { get; private set; }
        public ICommand ExportDataCommand { get; private set; }

        public SettingsDialogViewModel()
        {
            var serviceContainer = ServiceContainer.Instance;
            _configurationManager = serviceContainer.ConfigurationManager;
            _pathProvider = serviceContainer.PathProvider;
            _importExportManager = serviceContainer.ImportExportManager;
            
            InitializeCommands();
            
            // Subscribe to settings changes to update computed properties
            _configurationManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EnableTrash) || e.PropertyName == nameof(AutoEmptyTrash))
                {
                    OnPropertyChanged(nameof(ShowTrashRetention));
                }
            };
        }

        private void InitializeCommands()
        {
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            CloseCommand = new RelayCommand(Close);
            BrowseDataPathCommand = new RelayCommand(BrowseDataPath);
            ResetDataPathCommand = new RelayCommand(ResetDataPath);
            ImportDataCommand = new RelayCommand(ImportData);
            ExportDataCommand = new RelayCommand(ExportData);
        }

        public void InitializeForView()
        {
            // Refresh all property bindings
            OnPropertyChanged(string.Empty);
        }

        private void ResetToDefaults(object parameter)
        {
            _configurationManager.ResetToDefaults();
            
            // Refresh all bindings
            OnPropertyChanged(string.Empty);
        }

        private void Close(object parameter)
        {
            // Dialog will be closed by the dialog service
        }

        private void BrowseDataPath(object parameter)
        {
            var dialogManager = ServiceContainer.Instance.DialogManager;
            var selectedPath = dialogManager.ShowSaveFileDialog(
                "Choose Location for Account Data",
                "JSON files (*.json)|*.json|All files (*.*)|*.*",
                "accounts.json",
                "json");

            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (_pathProvider.SetCustomDataPath(selectedPath))
                {
                    // Also update the configuration manager
                    _configurationManager.CustomDataPath = selectedPath;
                    OnPropertyChanged(nameof(DataFileDisplayPath));
                }
                else
                {
                    DialogHelper.ShowError("The selected path is not valid or cannot be accessed.", "Invalid Path");
                }
            }
        }

        private void ResetDataPath(object parameter)
        {
            _pathProvider.ResetToDefaultPath();
            _configurationManager.CustomDataPath = null;
            OnPropertyChanged(nameof(DataFileDisplayPath));
        }

        private async void ImportData(object parameter)
        {
            await _importExportManager.ImportDataAsync();
        }

        private async void ExportData(object parameter)
        {
            await _importExportManager.ExportDataAsync();
        }
    }
}