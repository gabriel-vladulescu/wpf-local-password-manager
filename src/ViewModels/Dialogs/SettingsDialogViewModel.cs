using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Managers;
using AccountManager.Repositories;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class SettingsDialogViewModel : BaseViewModel
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IPathProvider _pathProvider;
        private readonly ImportExportManager _importExportManager;
        private readonly INotificationService _notificationService;
        private readonly AppDataRepository _dataRepository;

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

        public bool EnableApplicationNotifications
        {
            get => _configurationManager.EnableApplicationNotifications;
            set => _configurationManager.EnableApplicationNotifications = value;
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
            _notificationService = serviceContainer.NotificationService;
            _dataRepository = serviceContainer.DataRepository;
            
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
            
            // Show notification
            _notificationService.ShowSuccess("Settings have been reset to default values", "Settings Reset");
            
            // Refresh all bindings
            OnPropertyChanged(string.Empty);
        }

        private void Close(object parameter)
        {
            // Dialog will be closed by the dialog service
        }

        private async void BrowseDataPath(object parameter)
        {
            var dialogManager = ServiceContainer.Instance.DialogManager;
            var selectedFolder = dialogManager.ShowSelectFolderDialog(
                "Choose Location for Account Data",
                System.IO.Path.GetDirectoryName(_pathProvider.GetCurrentDataPath()));

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                // Get current state
                var currentPath = _pathProvider.GetCurrentDataPath();
                var isCurrentlyUsingDefault = _pathProvider.IsUsingDefaultPath();
                
                // Combine selected folder with accounts.json filename
                var newPath = Path.Combine(selectedFolder, "accounts.json");
                var defaultPath = _pathProvider.GetDefaultDataPath();
                
                // Normalize paths for comparison
                var normalizedNewPath = Path.GetFullPath(newPath);
                var normalizedDefaultPath = Path.GetFullPath(defaultPath);
                var isNewPathDefault = normalizedNewPath.Equals(normalizedDefaultPath, StringComparison.OrdinalIgnoreCase);
                
                // BUSINESS LOGIC IMPLEMENTATION:
                
                // Browse -> if save is to system default (AppData) -> no action
                if (isCurrentlyUsingDefault && isNewPathDefault)
                {
                    _notificationService.ShowInfo($"Data file is already located at: {_pathProvider.GetDisplayPath()}", "Same Location");
                    return;
                }
                
                // Browse -> if save is to a different folder than system default, move file to specified path, delete from appData
                if (isCurrentlyUsingDefault && !isNewPathDefault)
                {
                    await MoveTo_Custom_From_AppData(newPath, currentPath);
                    return;
                }
                
                // Browse -> if we have a custom path and we try to save to system default (AppData), file should be moved to AppData and removed from custom path
                if (!isCurrentlyUsingDefault && isNewPathDefault)
                {
                    await MoveTo_AppData_From_Custom(currentPath);
                    return;
                }
                
                // Browse -> if we have a custom path and try to save to the same custom path -> no action
                if (!isCurrentlyUsingDefault && !isNewPathDefault)
                {
                    // Check if it's the same path
                    var normalizedCurrentCustomPath = Path.GetFullPath(currentPath);
                    var normalizedNewCustomPath = Path.GetFullPath(newPath);
                    
                    if (normalizedCurrentCustomPath.Equals(normalizedNewCustomPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _notificationService.ShowInfo($"Data file is already located at: {_pathProvider.GetDisplayPath()}", "Same Location");
                        return;
                    }
                    
                    // Browse -> custom path to different custom path
                    await MoveTo_Custom_From_Custom(newPath, currentPath);
                    return;
                }
            }
        }

        private async void ResetDataPath(object parameter)
        {
            var isCurrentlyUsingDefault = _pathProvider.IsUsingDefaultPath();
            
            // BUSINESS LOGIC IMPLEMENTATION:
            
            // Reset -> if is already system default (AppData) -> no action
            if (isCurrentlyUsingDefault)
            {
                _notificationService.ShowInfo($"Data file is already located at: {_pathProvider.GetDisplayPath()}", "Already Default Location");
                return;
            }
            
            // Reset -> if is not system default (AppData), move file to AppData then delete it from custom path
            var currentCustomPath = _pathProvider.GetCurrentDataPath();
            await MoveTo_AppData_From_Custom(currentCustomPath);
        }

        private async void ImportData(object parameter)
        {
            // Close the settings dialog first to allow the loading overlay to show properly
            var dialogManager = ServiceContainer.Instance.DialogManager;
            dialogManager.CloseDialog();
            
            // Import with loading overlay
            await _importExportManager.ImportDataAsync();
        }

        // HELPER METHODS FOR PATH OPERATIONS
        
        private async Task MoveTo_Custom_From_AppData(string newPath, string currentPath)
        {
            // Get current data before switching
            var currentData = await _dataRepository.GetAsync();
            
            // Set new custom path
            if (await _pathProvider.SetCustomDataPathAsync(newPath))
            {
                _configurationManager.CustomDataPath = newPath;
                _dataRepository.InvalidateCache();
                
                // Save data to new custom location
                if (currentData != null)
                {
                    await _dataRepository.SaveAsync(currentData);
                    
                    // Delete from AppData
                    try
                    {
                        if (File.Exists(currentPath))
                        {
                            File.Delete(currentPath);
                        }
                        _notificationService.ShowInfo($"Data file moved to: {_pathProvider.GetDisplayPath()}\nOld AppData file deleted.", "Moved to Custom Location");
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowWarning($"Data file moved to: {_pathProvider.GetDisplayPath()}\nCould not delete old AppData file: {ex.Message}", "Moved to Custom Location");
                    }
                }
                
                OnPropertyChanged(nameof(DataFileDisplayPath));
            }
            else
            {
                DialogHelper.ShowError("The selected folder is not valid or cannot be accessed.", "Invalid Path");
            }
        }
        
        private async Task MoveTo_AppData_From_Custom(string currentCustomPath)
        {
            // Get current data before switching
            var currentData = await _dataRepository.GetAsync();
            
            // Reset to default AppData
            await _pathProvider.ResetToDefaultPathAsync();
            _configurationManager.CustomDataPath = null;
            _dataRepository.InvalidateCache();
            
            // Save data to AppData location
            if (currentData != null)
            {
                await _dataRepository.SaveAsync(currentData);
                
                // Delete from custom location
                try
                {
                    if (File.Exists(currentCustomPath))
                    {
                        File.Delete(currentCustomPath);
                    }
                    _notificationService.ShowInfo($"Data file moved to: {_pathProvider.GetDisplayPath()}\nOld custom file deleted.", "Moved to Default Location");
                }
                catch (Exception ex)
                {
                    _notificationService.ShowWarning($"Data file moved to: {_pathProvider.GetDisplayPath()}\nCould not delete old custom file: {ex.Message}", "Moved to Default Location");
                }
            }
            
            OnPropertyChanged(nameof(DataFileDisplayPath));
        }
        
        private async Task MoveTo_Custom_From_Custom(string newPath, string currentPath)
        {
            // Get current data before switching
            var currentData = await _dataRepository.GetAsync();
            
            // Set new custom path
            if (await _pathProvider.SetCustomDataPathAsync(newPath))
            {
                _configurationManager.CustomDataPath = newPath;
                _dataRepository.InvalidateCache();
                
                // Save data to new custom location
                if (currentData != null)
                {
                    await _dataRepository.SaveAsync(currentData);
                    
                    // Delete from old custom location
                    try
                    {
                        if (File.Exists(currentPath))
                        {
                            File.Delete(currentPath);
                        }
                        _notificationService.ShowInfo($"Data file moved to: {_pathProvider.GetDisplayPath()}\nOld custom file deleted.", "Moved to New Custom Location");
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowWarning($"Data file moved to: {_pathProvider.GetDisplayPath()}\nCould not delete old custom file: {ex.Message}", "Moved to New Custom Location");
                    }
                }
                
                OnPropertyChanged(nameof(DataFileDisplayPath));
            }
            else
            {
                DialogHelper.ShowError("The selected folder is not valid or cannot be accessed.", "Invalid Path");
            }
        }

        private async void ExportData(object parameter)
        {
            await _importExportManager.ExportDataAsync();
        }
    }
}