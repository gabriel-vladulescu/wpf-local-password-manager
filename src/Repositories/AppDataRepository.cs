using System;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Models;

namespace AccountManager.Repositories
{
    /// <summary>
    /// Repository for application data operations
    /// </summary>
    public class AppDataRepository : IRepository<AppData>
    {
        private readonly ISerializer _serializer;
        private readonly IPathProvider _pathProvider;
        private AppData _cachedData;

        public event EventHandler<AppData> DataChanged;

        public AppDataRepository(ISerializer serializer, IPathProvider pathProvider)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public async Task<AppData> GetAsync()
        {
            try
            {
                if (_cachedData == null)
                {
                    var filePath = _pathProvider.GetCurrentDataPath();
                    _cachedData = await _serializer.DeserializeAsync<AppData>(filePath);
                    
                    // Ensure data integrity
                    _cachedData?.Validate();
                    
                    System.Diagnostics.Debug.WriteLine("AppData loaded from repository");
                }
                
                return _cachedData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading AppData: {ex.Message}");
                _cachedData = new AppData();
                return _cachedData;
            }
        }

        public async Task<bool> SaveAsync(AppData entity)
        {
            try
            {
                if (entity == null)
                    return false;

                var filePath = _pathProvider.GetCurrentDataPath();
                var success = await _serializer.SerializeAsync(entity, filePath);
                
                if (success)
                {
                    _cachedData = entity;
                    DataChanged?.Invoke(this, entity);
                    System.Diagnostics.Debug.WriteLine("AppData saved to repository");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving AppData: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExistsAsync()
        {
            try
            {
                var data = await GetAsync();
                return data != null && data.HasData;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Imports data from a specific file path
        /// </summary>
        public async Task<AppData> ImportAsync(string filePath)
        {
            try
            {
                var importedData = await _serializer.DeserializeAsync<AppData>(filePath);
                if (importedData != null)
                {
                    importedData.Validate();
                    System.Diagnostics.Debug.WriteLine($"Data imported successfully from {filePath}");
                }
                return importedData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing data from {filePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports current data to a specific file path
        /// </summary>
        public async Task<bool> ExportAsync(string filePath)
        {
            try
            {
                var data = await GetAsync();
                if (data == null)
                    return false;

                var success = await _serializer.SerializeAsync(data, filePath);
                if (success)
                {
                    System.Diagnostics.Debug.WriteLine($"Data exported successfully to {filePath}");
                }
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting data to {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Invalidates the cache and forces reload on next access
        /// </summary>
        public void InvalidateCache()
        {
            _cachedData = null;
        }

        /// <summary>
        /// Updates cached data without saving to file
        /// </summary>
        public void UpdateCache(AppData data)
        {
            _cachedData = data;
            DataChanged?.Invoke(this, data);
        }
    }
}