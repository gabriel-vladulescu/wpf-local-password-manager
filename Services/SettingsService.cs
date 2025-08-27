using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AccountManager.Models;

namespace AccountManager.Services
{
    public class SettingsService : INotifyPropertyChanged
    {
        private static SettingsService _instance;
        private readonly DataManager _dataManager;

        public static SettingsService Instance => _instance ??= new SettingsService();

        // Privacy & Security Settings
        public bool CensorAccountData
        {
            get => _dataManager.CurrentData?.Settings?.CensorAccountData ?? false;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.CensorAccountData != value)
                {
                    _dataManager.CurrentData.Settings.CensorAccountData = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool CensorPassword
        {
            get => _dataManager.CurrentData?.Settings?.CensorPassword ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.CensorPassword != value)
                {
                    _dataManager.CurrentData.Settings.CensorPassword = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableEncryption
        {
            get => _dataManager.CurrentData?.Settings?.EnableEncryption ?? false;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.EnableEncryption != value)
                {
                    _dataManager.CurrentData.Settings.EnableEncryption = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // UI Settings
        public bool EnableLocalSearch
        {
            get => _dataManager.CurrentData?.Settings?.EnableLocalSearch ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.EnableLocalSearch != value)
                {
                    _dataManager.CurrentData.Settings.EnableLocalSearch = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Confirmation Settings
        public bool ConfirmAccountDelete
        {
            get => _dataManager.CurrentData?.Settings?.ConfirmAccountDelete ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.ConfirmAccountDelete != value)
                {
                    _dataManager.CurrentData.Settings.ConfirmAccountDelete = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _dataManager.CurrentData?.Settings?.ConfirmGroupDelete ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.ConfirmGroupDelete != value)
                {
                    _dataManager.CurrentData.Settings.ConfirmGroupDelete = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmArchiveAccount
        {
            get => _dataManager.CurrentData?.Settings?.ConfirmArchiveAccount ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.ConfirmArchiveAccount != value)
                {
                    _dataManager.CurrentData.Settings.ConfirmArchiveAccount = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Trash & Archive Settings
        public bool EnableTrash
        {
            get => _dataManager.CurrentData?.Settings?.EnableTrash ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.EnableTrash != value)
                {
                    _dataManager.CurrentData.Settings.EnableTrash = value;
                    SaveSettings();
                    OnPropertyChanged();
                    TrashSettingChanged?.Invoke(value);
                }
            }
        }

        public bool EnableArchive
        {
            get => _dataManager.CurrentData?.Settings?.EnableArchive ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.EnableArchive != value)
                {
                    _dataManager.CurrentData.Settings.EnableArchive = value;
                    SaveSettings();
                    OnPropertyChanged();
                    ArchiveSettingChanged?.Invoke(value);
                }
            }
        }

        public bool ShowFavoritesGroup
        {
            get => _dataManager.CurrentData?.Settings?.ShowFavoritesGroup ?? true;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.ShowFavoritesGroup != value)
                {
                    _dataManager.CurrentData.Settings.ShowFavoritesGroup = value;
                    SaveSettings();
                    OnPropertyChanged();
                    FavoritesVisibilityChanged?.Invoke(value);
                }
            }
        }

        public int TrashRetentionDays
        {
            get => _dataManager.CurrentData?.Settings?.TrashRetentionDays ?? 30;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.TrashRetentionDays != value)
                {
                    _dataManager.CurrentData.Settings.TrashRetentionDays = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoEmptyTrash
        {
            get => _dataManager.CurrentData?.Settings?.AutoEmptyTrash ?? false;
            set
            {
                if (_dataManager.CurrentData?.Settings != null && _dataManager.CurrentData.Settings.AutoEmptyTrash != value)
                {
                    _dataManager.CurrentData.Settings.AutoEmptyTrash = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        // Events for settings changes
        public event Action<bool> TrashSettingChanged;
        public event Action<bool> ArchiveSettingChanged;
        public event Action<bool> FavoritesVisibilityChanged;

        private SettingsService()
        {
            _dataManager = DataManager.Instance;
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                // Ensure settings are initialized in the shared data
                _dataManager.CurrentData.Settings ??= new AppSettings();
                _dataManager.CurrentData.Theme ??= new ThemeSettings();
                
                // Notify all properties changed
                OnPropertyChanged(string.Empty);
                
                System.Diagnostics.Debug.WriteLine("Settings loaded from shared DataManager");
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
                _dataManager.SaveCurrentData();
                System.Diagnostics.Debug.WriteLine("Settings saved via DataManager");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            if (_dataManager.CurrentData?.Settings != null)
            {
                _dataManager.CurrentData.Settings = new AppSettings(); // This will use default values
                SaveSettings();
                OnPropertyChanged(string.Empty);
            }
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
}