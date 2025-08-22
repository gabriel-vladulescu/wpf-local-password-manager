using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AccountManager.Services
{
    public class SettingsService : INotifyPropertyChanged
    {
        private static SettingsService _instance;
        private readonly string _settingsPath;

        // Private backing fields
        private bool _censorAccountData = false;
        private bool _censorPassword = true;
        private bool _enableEncryption = false;
        private bool _enableLocalSearch = true;
        private bool _confirmAccountDelete = true;
        private bool _confirmGroupDelete = true;
        private bool _enableTrash = true;
        private bool _enableArchive = true;
        private bool _showFavoritesGroup = true;
        private int _trashRetentionDays = 30;
        private bool _autoEmptyTrash = false;

        public static SettingsService Instance => _instance ??= new SettingsService();

        // Privacy & Security Settings
        public bool CensorAccountData
        {
            get => _censorAccountData;
            set
            {
                if (SetProperty(ref _censorAccountData, value))
                    SaveSettings();
            }
        }

        public bool CensorPassword
        {
            get => _censorPassword;
            set
            {
                if (SetProperty(ref _censorPassword, value))
                    SaveSettings();
            }
        }

        public bool EnableEncryption
        {
            get => _enableEncryption;
            set
            {
                if (SetProperty(ref _enableEncryption, value))
                    SaveSettings();
            }
        }

        // UI Settings
        public bool EnableLocalSearch
        {
            get => _enableLocalSearch;
            set
            {
                if (SetProperty(ref _enableLocalSearch, value))
                    SaveSettings();
            }
        }

        // Confirmation Settings
        public bool ConfirmAccountDelete
        {
            get => _confirmAccountDelete;
            set
            {
                if (SetProperty(ref _confirmAccountDelete, value))
                    SaveSettings();
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _confirmGroupDelete;
            set
            {
                if (SetProperty(ref _confirmGroupDelete, value))
                    SaveSettings();
            }
        }

        // Trash & Archive Settings
        public bool EnableTrash
        {
            get => _enableTrash;
            set
            {
                if (SetProperty(ref _enableTrash, value))
                {
                    SaveSettings();
                    TrashSettingChanged?.Invoke(value);
                }
            }
        }

        public bool EnableArchive
        {
            get => _enableArchive;
            set
            {
                if (SetProperty(ref _enableArchive, value))
                {
                    SaveSettings();
                    ArchiveSettingChanged?.Invoke(value);
                }
            }
        }

        public bool ShowFavoritesGroup
        {
            get => _showFavoritesGroup;
            set
            {
                if (SetProperty(ref _showFavoritesGroup, value))
                {
                    SaveSettings();
                    FavoritesVisibilityChanged?.Invoke(value);
                }
            }
        }

        public int TrashRetentionDays
        {
            get => _trashRetentionDays;
            set
            {
                if (SetProperty(ref _trashRetentionDays, value))
                    SaveSettings();
            }
        }

        public bool AutoEmptyTrash
        {
            get => _autoEmptyTrash;
            set
            {
                if (SetProperty(ref _autoEmptyTrash, value))
                    SaveSettings();
            }
        }

        // Events for settings changes
        public event Action<bool> TrashSettingChanged;
        public event Action<bool> ArchiveSettingChanged;
        public event Action<bool> FavoritesVisibilityChanged;

        private SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "AccountManager");
            
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);
                
            _settingsPath = Path.Combine(appFolder, "settings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    
                    if (settings != null)
                    {
                        _censorAccountData = settings.CensorAccountData;
                        _censorPassword = settings.CensorPassword;
                        _enableEncryption = settings.EnableEncryption;
                        _enableLocalSearch = settings.EnableLocalSearch;
                        _confirmAccountDelete = settings.ConfirmAccountDelete;
                        _confirmGroupDelete = settings.ConfirmGroupDelete;
                        _enableTrash = settings.EnableTrash;
                        _enableArchive = settings.EnableArchive;
                        _trashRetentionDays = settings.TrashRetentionDays;
                        _autoEmptyTrash = settings.AutoEmptyTrash;
                        _showFavoritesGroup = settings.ShowFavoritesGroup; // ADD THIS LINE

                        // Notify all properties changed
                        OnPropertyChanged(string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public void SaveSettings()
        {
            try
            {
                var settings = new SettingsData
                {
                    CensorAccountData = _censorAccountData,
                    CensorPassword = _censorPassword,
                    EnableEncryption = _enableEncryption,
                    EnableLocalSearch = _enableLocalSearch,
                    ConfirmAccountDelete = _confirmAccountDelete,
                    ConfirmGroupDelete = _confirmGroupDelete,
                    EnableTrash = _enableTrash,
                    EnableArchive = _enableArchive,
                    TrashRetentionDays = _trashRetentionDays,
                    AutoEmptyTrash = _autoEmptyTrash,
                    ShowFavoritesGroup = _showFavoritesGroup // ADD THIS LINE
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            _censorAccountData = false;
            _censorPassword = true;
            _enableEncryption = false;
            _enableLocalSearch = true;
            _confirmAccountDelete = true;
            _confirmGroupDelete = true;
            _enableTrash = true;
            _enableArchive = true;
            _trashRetentionDays = 30;
            _autoEmptyTrash = false;
            _showFavoritesGroup = true; // ADD THIS LINE

            SaveSettings();
            OnPropertyChanged(string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // Data class for JSON serialization
    public class SettingsData
    {
        public bool CensorAccountData { get; set; } = false;
        public bool CensorPassword { get; set; } = true;
        public bool EnableEncryption { get; set; } = false;
        public bool EnableLocalSearch { get; set; } = true;
        public bool ConfirmAccountDelete { get; set; } = true;
        public bool ConfirmGroupDelete { get; set; } = true;
        public bool EnableTrash { get; set; } = true;
        public bool EnableArchive { get; set; } = true;
        public int TrashRetentionDays { get; set; } = 30;
        public bool AutoEmptyTrash { get; set; } = false;
        public bool ShowFavoritesGroup { get; set; } = true; // ADD THIS LINE
    }
}