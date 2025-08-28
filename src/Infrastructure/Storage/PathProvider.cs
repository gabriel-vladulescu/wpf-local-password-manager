using System;
using System.IO;
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

        public PathProvider(IFileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
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

        public bool SetCustomDataPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _customDataPath = null;
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
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting custom data path: {ex.Message}");
                return false;
            }
        }

        public void ResetToDefaultPath()
        {
            SetCustomDataPath(null);
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