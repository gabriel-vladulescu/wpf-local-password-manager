using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Models;
using AccountManager.Repositories;

namespace AccountManager.Managers
{
    /// <summary>
    /// Manages the overall application state and data operations
    /// Replaces the old DataManager with better separation of concerns
    /// </summary>
    public class ApplicationStateManager
    {
        private static ApplicationStateManager _instance;
        private readonly AppDataRepository _dataRepository;
        
        public static ApplicationStateManager Instance => _instance;
        
        public event EventHandler<AppData> DataChanged;

        // Private constructor to prevent direct instantiation
        private ApplicationStateManager(AppDataRepository dataRepository)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _dataRepository.DataChanged += (sender, data) => DataChanged?.Invoke(this, data);
        }

        /// <summary>
        /// Initializes the singleton instance with dependencies
        /// </summary>
        public static void Initialize(AppDataRepository dataRepository)
        {
            if (_instance == null)
            {
                _instance = new ApplicationStateManager(dataRepository);
            }
        }

        /// <summary>
        /// Gets the current application data
        /// </summary>
        public async Task<AppData> GetCurrentDataAsync()
        {
            return await _dataRepository.GetAsync();
        }

        /// <summary>
        /// Saves the current application data
        /// </summary>
        public async Task<bool> SaveDataAsync(AppData data)
        {
            return await _dataRepository.SaveAsync(data);
        }

        /// <summary>
        /// Updates the Groups collection and saves to storage
        /// </summary>
        public async Task<bool> UpdateGroupsAsync(List<AccountGroup> groups)
        {
            var data = await _dataRepository.GetAsync();
            if (data != null)
            {
                data.Groups = groups ?? new List<AccountGroup>();
                return await _dataRepository.SaveAsync(data);
            }
            return false;
        }

        /// <summary>
        /// Synchronizes Groups data from ViewModels to ensure consistency
        /// </summary>
        public async Task SyncGroupsFromViewModelsAsync()
        {
            try
            {
                var app = System.Windows.Application.Current;
                if (app?.MainWindow?.DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    if (mainViewModel.Groups?.Any() == true)
                    {
                        await UpdateGroupsAsync(mainViewModel.Groups.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not sync Groups from ViewModels: {ex.Message}");
            }
        }

        /// <summary>
        /// Reloads data from storage
        /// </summary>
        public async Task ReloadDataAsync()
        {
            _dataRepository.InvalidateCache();
            var data = await _dataRepository.GetAsync();
            DataChanged?.Invoke(this, data);
        }

        /// <summary>
        /// Checks if data exists in storage
        /// </summary>
        public async Task<bool> DataExistsAsync()
        {
            return await _dataRepository.ExistsAsync();
        }

        /// <summary>
        /// Gets the current data file path being used
        /// </summary>
        public string GetCurrentDataPath()
        {
            // This will need to be injected or accessed through the path provider
            throw new NotImplementedException("Path access needs to be implemented through dependency injection");
        }
    }
}