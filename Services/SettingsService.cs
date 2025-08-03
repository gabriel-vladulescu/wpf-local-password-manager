using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AccountManager.Services.Interfaces;
using AccountManager.Utilities.Constants;
using AccountManager.Utilities.Helpers;

namespace AccountManager.Services
{
    public class SettingsService : ISettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private bool _censorAccountData = false;
        private bool _censorPassword = true;
        private bool _enableEncryption = false;
        private bool _enableLocalSearch = true;
        private bool _confirmGroupDelete = true;
        private bool _confirmAccountDelete = true;
        private bool _confirmAccountEdit = true;
        private bool _autoSave = true;
        private int _autoSaveInterval = 5; // minutes

        #region Properties

        public bool CensorAccountData
        {
            get => _censorAccountData;
            set
            {
                if (SetProperty(ref _censorAccountData, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool CensorPassword
        {
            get => _censorPassword;
            set
            {
                if (SetProperty(ref _censorPassword, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool EnableEncryption
        {
            get => _enableEncryption;
            set
            {
                if (SetProperty(ref _enableEncryption, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool EnableLocalSearch
        {
            get => _enableLocalSearch;
            set
            {
                if (SetProperty(ref _enableLocalSearch, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _confirmGroupDelete;
            set
            {
                if (SetProperty(ref _confirmGroupDelete, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool ConfirmAccountDelete
        {
            get => _confirmAccountDelete;
            set
            {
                if (SetProperty(ref _confirmAccountDelete, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool ConfirmAccountEdit
        {
            get => _confirmAccountEdit;
            set
            {
                if (SetProperty(ref _confirmAccountEdit, value))
                {
                    SaveSettings();
                }
            }
        }

        public bool AutoSave
        {
            get => _autoSave;
            set
            {
                if (SetProperty(ref _autoSave, value))
                {
                    SaveSettings();
                }
            }
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                if (SetProperty(ref _autoSaveInterval, Math.Max(1, Math.Min(60, value))))
                {
                    SaveSettings();
                }
            }
        }

        #endregion

        private SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                var json = FileHelper.SafeReadAllTextAsync(AppConstants.SettingsFileName).Result;
                if (!string.IsNullOrEmpty(json))
                {
                    var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _censorAccountData = settings.CensorAccountData;
                        _censorPassword = settings.CensorPassword;
                        _enableEncryption = settings.EnableEncryption;
                        _enableLocalSearch = settings.EnableLocalSearch;
                        _confirmGroupDelete = settings.ConfirmGroupDelete;
                        _confirmAccountDelete = settings.ConfirmAccountDelete;
                        _confirmAccountEdit = settings.ConfirmAccountEdit;
                        _autoSave = settings.AutoSave;
                        _autoSaveInterval = settings.AutoSaveInterval;

                        // Notify all properties changed
                        OnPropertyChanged("");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    CensorAccountData = _censorAccountData,
                    CensorPassword = _censorPassword,
                    EnableEncryption = _enableEncryption,
                    EnableLocalSearch = _enableLocalSearch,
                    ConfirmGroupDelete = _confirmGroupDelete,
                    ConfirmAccountDelete = _confirmAccountDelete,
                    ConfirmAccountEdit = _confirmAccountEdit,
                    AutoSave = _autoSave,
                    AutoSaveInterval = _autoSaveInterval
                };

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(settings, options);
                
                _ = FileHelper.SafeWriteAllTextAsync(AppConstants.SettingsFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            _censorAccountData = false;
            _censorPassword = true;
            _enableEncryption = false;
            _enableLocalSearch = true;
            _confirmGroupDelete = true;
            _confirmAccountDelete = true;
            _confirmAccountEdit = true;
            _autoSave = true;
            _autoSaveInterval = 5;

            SaveSettings();
            OnPropertyChanged(""); // Notify all properties changed
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    internal class AppSettings
    {
        public bool CensorAccountData { get; set; }
        public bool CensorPassword { get; set; } = true;
        public bool EnableEncryption { get; set; }
        public bool EnableLocalSearch { get; set; } = true;
        public bool ConfirmGroupDelete { get; set; } = true;
        public bool ConfirmAccountDelete { get; set; } = true;
        public bool ConfirmAccountEdit { get; set; } = true;
        public bool AutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 5;
    }
}