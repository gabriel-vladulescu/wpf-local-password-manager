using System;
using System.Linq;
using AccountManager.Models;

namespace AccountManager.Services
{
    /// <summary>
    /// Centralized data manager that maintains a single source of truth for all application data.
    /// Prevents data loss when different services need to save changes.
    /// </summary>
    public class DataManager
    {
        private static DataManager _instance;
        private readonly JsonService _jsonService;
        private AccountData _currentData;

        public static DataManager Instance => _instance ??= new DataManager();

        public event EventHandler<AccountData> DataChanged;

        private DataManager()
        {
            _jsonService = new JsonService();
            LoadData();
        }

        /// <summary>
        /// Gets the current data instance. All services should use this same instance.
        /// </summary>
        public AccountData CurrentData => _currentData;

        /// <summary>
        /// Loads data from disk and updates the current instance.
        /// </summary>
        public void LoadData()
        {
            try
            {
                _currentData = _jsonService.LoadData();
                System.Diagnostics.Debug.WriteLine("DataManager: Data loaded from disk");
                DataChanged?.Invoke(this, _currentData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataManager: Error loading data: {ex.Message}");
                // Only fallback to empty data if we don't already have data
                if (_currentData == null)
                {
                    _currentData = new AccountData(); // Fallback to empty data only if needed
                    System.Diagnostics.Debug.WriteLine("DataManager: Using empty fallback data");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DataManager: Keeping existing data due to load error");
                }
            }
        }

        /// <summary>
        /// Saves the current data instance to disk.
        /// This preserves all data including Groups, Settings, and Theme.
        /// </summary>
        public void SaveData()
        {
            try
            {
                if (_currentData != null)
                {
                    _jsonService.SaveData(_currentData);
                    DataChanged?.Invoke(this, _currentData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataManager: Error saving data: {ex.Message}");
                throw; // Re-throw so calling code can handle the error
            }
        }

        /// <summary>
        /// Updates the Groups collection and saves to disk.
        /// Used by ViewModels when account/group data changes.
        /// </summary>
        public void UpdateGroups(System.Collections.Generic.List<AccountGroup> groups)
        {
            if (_currentData != null)
            {
                _currentData.Groups = groups;
                SaveData();
            }
        }

        /// <summary>
        /// Gets the latest Groups data from ViewModels (if available) and saves.
        /// Used by Settings and Theme services when their data changes.
        /// </summary>
        public void SaveCurrentData()
        {
            // Ensure we have the latest Groups data
            SyncGroupsFromViewModels();
            SaveData();
        }

        /// <summary>
        /// Synchronizes Groups data from ViewModels to ensure we have the latest data.
        /// This prevents data loss when Settings/Theme services save.
        /// </summary>
        private void SyncGroupsFromViewModels()
        {
            try
            {
                // Try to get the current MainViewModel instance and sync Groups
                // This is a bit hacky but ensures we don't lose Groups data
                var app = System.Windows.Application.Current;
                if (app?.MainWindow?.DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    if (mainViewModel.Groups?.Any() == true)
                    {
                        _currentData.Groups = mainViewModel.Groups.ToList();
                        System.Diagnostics.Debug.WriteLine("DataManager: Synced Groups from MainViewModel");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DataManager: Could not sync Groups from ViewModels: {ex.Message}");
                // Not critical - continue with existing data
            }
        }
    }
}