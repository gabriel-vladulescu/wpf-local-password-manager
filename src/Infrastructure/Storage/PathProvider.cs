using System;
using System.IO;
using System.Threading.Tasks;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Config;

namespace AccountManager.Infrastructure.Storage
{
    /// <summary>
    /// Provides file paths for data storage with custom path support
    /// </summary>
    public class PathProvider : IPathProvider
    {
        private readonly IFileStorage _fileStorage;
        private string _customDataPath;
        private string _configFilePath;

        public PathProvider(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            InitializeConfigPath();
            LoadCustomDataPathSync(); // Load synchronously to ensure it's available immediately
        }

        private void InitializeConfigPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, AppConfig.Application.Name);
            _fileStorage.CreateDirectory(appFolderPath);
            _configFilePath = Path.Combine(appFolderPath, "app-config.json");
        }

        private void LoadCustomDataPathSync()
        {
            try
            {
                if (_fileStorage.Exists(_configFilePath))
                {
                    // Read file content synchronously for startup performance
                    // This is safe because it's just reading a small config file
                    var configContent = System.IO.File.ReadAllText(_configFilePath);
                    if (!string.IsNullOrWhiteSpace(configContent))
                    {
                        var config = System.Text.Json.JsonSerializer.Deserialize<PathConfig>(configContent);
                        _customDataPath = config?.CustomDataPath;
                        
                        // Don't validate path here - just load the configuration
                        // Validation will happen when the path is actually used
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default path
                System.Diagnostics.Debug.WriteLine($"Error loading path configuration synchronously: {ex.Message}");
                _customDataPath = null;
            }
        }

        public async Task LoadCustomDataPathAsync()
        {
            // Path was already loaded synchronously in constructor
            // This method now just validates the path if it exists
            try
            {
                if (!string.IsNullOrEmpty(_customDataPath) && !_fileStorage.ValidatePath(_customDataPath))
                {
                    // If custom path is invalid, reset to null (use default)
                    System.Diagnostics.Debug.WriteLine($"Custom path '{_customDataPath}' is not valid, reverting to default");
                    _customDataPath = null;
                    
                    // Save the corrected configuration
                    await SaveCustomDataPathAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default path
                System.Diagnostics.Debug.WriteLine($"Error validating path configuration: {ex.Message}");
                _customDataPath = null;
            }
        }

        private async Task SaveCustomDataPathAsync()
        {
            try
            {
                var config = new PathConfig { CustomDataPath = _customDataPath };
                var configContent = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await _fileStorage.WriteTextAsync(_configFilePath, configContent);
            }
            catch (Exception ex)
            {
                // Don't access ServiceContainer during initialization to avoid circular dependency  
                System.Diagnostics.Debug.WriteLine($"Error saving path configuration: {ex.Message}");
            }
        }

        private class PathConfig
        {
            public string CustomDataPath { get; set; }
        }

        public string GetDefaultDataPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, AppConfig.Application.Name);
            
            _fileStorage.CreateDirectory(appFolderPath);
            
            return Path.Combine(appFolderPath, AppConfig.Storage.DataFileName);
        }

        public string GetCurrentDataPath()
        {
            var customPath = GetCustomDataPath();
            
            if (!string.IsNullOrEmpty(customPath) && _fileStorage.ValidatePath(customPath))
            {
                return customPath;
            }
            
            return GetDefaultDataPath();
        }

        public string GetCustomDataPath()
        {
            return _customDataPath;
        }

        public async Task<bool> SetCustomDataPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _customDataPath = null;
                await SaveCustomDataPathAsync();
                return true;
            }

            if (!_fileStorage.ValidatePath(path))
            {
                return false;
            }

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    _fileStorage.CreateDirectory(directory);
                }

                _customDataPath = path;
                await SaveCustomDataPathAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error for path setting issues
                System.Diagnostics.Debug.WriteLine($"Error setting custom data path: {ex.Message}");
                return false;
            }
        }

        public async Task ResetToDefaultPathAsync()
        {
            await SetCustomDataPathAsync(null);
        }

        public bool IsUsingDefaultPath()
        {
            return string.IsNullOrEmpty(_customDataPath);
        }

        public string GetDisplayPath()
        {
            var currentPath = GetCurrentDataPath();
            var defaultPath = GetDefaultDataPath();

            if (currentPath.Equals(defaultPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Default (AppData)";
            }

            return currentPath;
        }

        public string GetDataDirectory()
        {
            return Path.GetDirectoryName(GetCurrentDataPath());
        }
    }
}