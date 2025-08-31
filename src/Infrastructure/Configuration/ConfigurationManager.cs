using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Models;
using AccountManager.Repositories;

namespace AccountManager.Infrastructure.Configuration
{
    /// <summary>
    /// Manages application configuration and settings
    /// </summary>
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly IRepository<AppData> _dataRepository;
        private readonly IEncryptionConfigManager _encryptionConfigManager;
        private AppSettings _settings;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<bool> TrashSettingChanged;
        public event Action<bool> ArchiveSettingChanged;
        public event Action<bool> FavoritesVisibilityChanged;

        public ConfigurationManager(IRepository<AppData> dataRepository, IEncryptionConfigManager encryptionConfigManager = null)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _encryptionConfigManager = encryptionConfigManager; // Can be null now since we store encryption settings in AppConfig
            
            // Subscribe to data changes to reload settings when data is imported/changed
            if (_dataRepository is AppDataRepository appDataRepository)
            {
                appDataRepository.DataChanged += OnDataChanged;
            }
            
            // Note: LoadSettings() will be called manually after encryption is ready
        }

        // Privacy & Security Settings
        public bool CensorAccountData
        {
            get => _settings?.CensorAccountData ?? false;
            set
            {
                if (_settings != null && _settings.CensorAccountData != value)
                {
                    _settings.CensorAccountData = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool CensorPassword
        {
            get => _settings?.CensorPassword ?? true;
            set
            {
                if (_settings != null && _settings.CensorPassword != value)
                {
                    _settings.CensorPassword = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableEncryption
        {
            get => _settings?.EnableEncryption ?? false;
            set
            {
                if (_settings != null && _settings.EnableEncryption != value)
                {
                    _settings.EnableEncryption = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // UI Settings
        public bool EnableLocalSearch
        {
            get => _settings?.EnableLocalSearch ?? true;
            set
            {
                if (_settings != null && _settings.EnableLocalSearch != value)
                {
                    _settings.EnableLocalSearch = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableApplicationNotifications
        {
            get => _settings?.EnableApplicationNotifications ?? true;
            set
            {
                if (_settings != null && _settings.EnableApplicationNotifications != value)
                {
                    _settings.EnableApplicationNotifications = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Confirmation Settings
        public bool ConfirmAccountDelete
        {
            get => _settings?.ConfirmAccountDelete ?? true;
            set
            {
                if (_settings != null && _settings.ConfirmAccountDelete != value)
                {
                    _settings.ConfirmAccountDelete = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _settings?.ConfirmGroupDelete ?? true;
            set
            {
                if (_settings != null && _settings.ConfirmGroupDelete != value)
                {
                    _settings.ConfirmGroupDelete = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmArchiveAccount
        {
            get => _settings?.ConfirmArchiveAccount ?? true;
            set
            {
                if (_settings != null && _settings.ConfirmArchiveAccount != value)
                {
                    _settings.ConfirmArchiveAccount = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Trash & Archive Settings
        public bool EnableTrash
        {
            get => _settings?.EnableTrash ?? true;
            set
            {
                if (_settings != null && _settings.EnableTrash != value)
                {
                    _settings.EnableTrash = value;
                    SaveSettings();
                    OnPropertyChanged();
                    TrashSettingChanged?.Invoke(value);
                }
            }
        }

        public bool EnableArchive
        {
            get => _settings?.EnableArchive ?? true;
            set
            {
                if (_settings != null && _settings.EnableArchive != value)
                {
                    _settings.EnableArchive = value;
                    SaveSettings();
                    OnPropertyChanged();
                    ArchiveSettingChanged?.Invoke(value);
                }
            }
        }

        public bool ShowFavoritesGroup
        {
            get => _settings?.ShowFavoritesGroup ?? true;
            set
            {
                if (_settings != null && _settings.ShowFavoritesGroup != value)
                {
                    _settings.ShowFavoritesGroup = value;
                    SaveSettings();
                    OnPropertyChanged();
                    FavoritesVisibilityChanged?.Invoke(value);
                }
            }
        }

        public int TrashRetentionDays
        {
            get => _settings?.TrashRetentionDays ?? 30;
            set
            {
                if (_settings != null && _settings.TrashRetentionDays != value)
                {
                    _settings.TrashRetentionDays = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoEmptyTrash
        {
            get => _settings?.AutoEmptyTrash ?? false;
            set
            {
                if (_settings != null && _settings.AutoEmptyTrash != value)
                {
                    _settings.AutoEmptyTrash = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Data File Settings
        public string CustomDataPath
        {
            get => _settings?.CustomDataPath;
            set
            {
                if (_settings != null && _settings.CustomDataPath != value)
                {
                    _settings.CustomDataPath = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }


        public async void LoadSettings()
        {
            await LoadSettingsAsync();
        }

        /// <summary>
        /// Reloads settings after encryption is available
        /// </summary>
        public async Task ReloadSettingsAfterEncryptionAsync()
        {
            await LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                // Try to get data, but if encryption is not ready yet, use default settings
                AppData data = null;
                try
                {
                    data = await _dataRepository.GetAsync();
                }
                catch (Exception dataEx) when (dataEx.Message.Contains("No passphrase available"))
                {
                    // Encryption not ready yet - use default settings
                    System.Diagnostics.Debug.WriteLine("ConfigurationManager: Encryption not ready, using default settings");
                    _settings = new AppSettings();
                    OnPropertyChanged(string.Empty);
                    return;
                }
                
                _settings = data?.Settings ?? new AppSettings();
                OnPropertyChanged(string.Empty);
            }
            catch (Exception ex)
            {
                // Try to show error notification, but don't fail if NotificationService isn't available
                try
                {
                    var notificationService = ServiceContainer.Instance.NotificationService;
                    notificationService?.ShowError($"Error loading settings: {ex.Message}", "Settings Error");
                }
                catch { } // Prevent recursive errors during initialization
                _settings = new AppSettings();
            }
        }

        public async void SaveSettings()
        {
            try
            {
                var data = await _dataRepository.GetAsync();
                if (data != null)
                {
                    data.Settings = _settings;
                    await _dataRepository.SaveAsync(data);
                }
            }
            catch (Exception ex)
            {
                // Try to show error notification
                try
                {
                    var notificationService = ServiceContainer.Instance.NotificationService;
                    notificationService?.ShowError($"Error saving settings: {ex.Message}", "Save Settings Error");
                }
                catch { } // Prevent recursive errors
            }
        }

        public void ResetToDefaults()
        {
            _settings = new AppSettings();
            SaveSettings();
            OnPropertyChanged(string.Empty);
        }

        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            SaveSettings();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handle data changes from repository (e.g., import operations)
        /// </summary>
        private void OnDataChanged(object sender, AppData newData)
        {
            try
            {
                // Reload settings when data changes (e.g., after import) - ensure we're on the UI thread
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    LoadSettings();
                }));
            }
            catch (Exception ex)
            {
                // Log error but don't fail the settings system
                try
                {
                    var notificationService = ServiceContainer.Instance.NotificationService;
                    notificationService?.ShowError($"Error reloading settings after data change: {ex.Message}", "Settings Reload Error");
                }
                catch { }
            }
        }
    }
}