using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Config;

namespace AccountManager.Infrastructure.Configuration
{
    /// <summary>
    /// Manages encryption detection by examining file content rather than storing config
    /// </summary>
    public class AppConfigEncryptionManager : IEncryptionConfigManager
    {
        private readonly IPathProvider _pathProvider;

        public AppConfigEncryptionManager(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public async Task<bool> IsEncryptionEnabledAsync()
        {
            await Task.CompletedTask;
            
            // Check if accounts file exists and contains encrypted data
            var accountsFilePath = _pathProvider.GetCurrentDataPath();
            System.Diagnostics.Debug.WriteLine($"AppConfigEncryptionManager: Checking file at path: {accountsFilePath}");
            
            if (!File.Exists(accountsFilePath))
            {
                System.Diagnostics.Debug.WriteLine("AppConfigEncryptionManager: File does not exist, returning false");
                return false; // No file means no encryption
            }

            try
            {
                var content = await File.ReadAllTextAsync(accountsFilePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    System.Diagnostics.Debug.WriteLine("AppConfigEncryptionManager: File is empty, returning false");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"AppConfigEncryptionManager: File content preview: {content.Substring(0, Math.Min(100, content.Length))}...");

                // Parse as JSON and check structure
                using (var document = JsonDocument.Parse(content))
                {
                    var root = document.RootElement;
                    
                    bool hasData = root.TryGetProperty("data", out _);
                    bool hasVersion = root.TryGetProperty("version", out _);
                    
                    System.Diagnostics.Debug.WriteLine($"AppConfigEncryptionManager: Has 'data' property: {hasData}, Has 'version' property: {hasVersion}");
                    
                    // If it has "data" and "version" properties, it's encrypted (EncryptedContainer)
                    if (hasData && hasVersion)
                    {
                        System.Diagnostics.Debug.WriteLine("AppConfigEncryptionManager: File is encrypted, returning true");
                        return true;
                    }
                    
                    // If it has normal AppData structure (Accounts, Groups, etc.), it's not encrypted
                    System.Diagnostics.Debug.WriteLine("AppConfigEncryptionManager: File is not encrypted, returning false");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppConfigEncryptionManager: Exception parsing file: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetSaltAsync()
        {
            await Task.CompletedTask;
            return AppConfig.Encryption.Salt; // Return constant salt
        }

        public async Task<string> GetTestPayloadAsync()
        {
            await Task.CompletedTask;
            return null; // No test payload needed with constant salt
        }
    }
}