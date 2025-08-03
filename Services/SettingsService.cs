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
        private const string SettingsFileName = "app-settings.json";
        
        private bool _censorAccountData = false;
        private bool _censorPassword = true;
        private bool _enableEncryption = false;
        private bool _enableLocalSearch = true;
        private bool _confirmGroupDelete = true;
        private bool _confirmAccountDelete = true;
        private bool _confirmAccountEdit = true;

        public static SettingsService Instance => _instance ??= new SettingsService();

        public bool CensorAccountData
        {
            get => _censorAccountData;
            set
            {
                _censorAccountData = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool CensorPassword
        {
            get => _censorPassword;
            set
            {
                _censorPassword = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool EnableEncryption
        {
            get => _enableEncryption;
            set
            {
                _enableEncryption = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool EnableLocalSearch
        {
            get => _enableLocalSearch;
            set
            {
                _enableLocalSearch = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _confirmGroupDelete;
            set
            {
                _confirmGroupDelete = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool ConfirmAccountDelete
        {
            get => _confirmAccountDelete;
            set
            {
                _confirmAccountDelete = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public bool ConfirmAccountEdit
        {
            get => _confirmAccountEdit;
            set
            {
                _confirmAccountEdit = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        private SettingsService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    var json = File.ReadAllText(SettingsFileName);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _censorAccountData = settings.CensorAccountData;
                        _censorPassword = settings.CensorPassword;
                        _enableEncryption = settings.EnableEncryption;
                        _enableLocalSearch = settings.EnableLocalSearch;
                        _confirmGroupDelete = settings.ConfirmGroupDelete;
                        _confirmAccountDelete = settings.ConfirmAccountDelete;
                        _confirmAccountEdit = settings.ConfirmAccountEdit;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void SaveSettings()
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
                    ConfirmAccountEdit = _confirmAccountEdit
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
    }
}