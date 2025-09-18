using System;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Exceptions;
using AccountManager.Config;

namespace AccountManager.Infrastructure.Serialization
{
    /// <summary>
    /// Serializer wrapper that encrypts/decrypts data based on configuration
    /// </summary>
    public class EncryptedJsonSerializer : ISerializer
    {
        private readonly ISerializer _baseSerializer;
        private readonly IEncryptionService _encryptionService;
        private readonly IEncryptionConfigManager _encryptionConfigManager;

        public EncryptedJsonSerializer(
            ISerializer baseSerializer,
            IEncryptionService encryptionService,
            IEncryptionConfigManager encryptionConfigManager)
        {
            _baseSerializer = baseSerializer ?? throw new ArgumentNullException(nameof(baseSerializer));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _encryptionConfigManager = encryptionConfigManager ?? throw new ArgumentNullException(nameof(encryptionConfigManager));
        }

        public async Task<T> DeserializeAsync<T>(string filePath) where T : class, new()
        {
            try
            {
                // First check if file exists and what format it's in
                var isFileEncrypted = await _encryptionConfigManager.IsEncryptionEnabledAsync();
                if (!isFileEncrypted)
                {
                    // File is in unencrypted format - use base serializer directly
                    return await _baseSerializer.DeserializeAsync<T>(filePath);
                }

                // Read encrypted content
                var encryptedContent = await _baseSerializer.DeserializeAsync<EncryptedContainer>(filePath);
                
                if (encryptedContent?.Data == null)
                {
                    // File exists but no encrypted data - return default
                    return new T();
                }

                // Decrypt the content (salt is now constant - no need to set)
                
                // Note: This will throw if wrong passphrase - calling code should handle
                var decryptedJson = await _encryptionService.DecryptAsync(encryptedContent.Data, GetCurrentPassphrase());
                
                // Deserialize the decrypted JSON
                return _baseSerializer.Deserialize<T>(decryptedJson);
            }
            catch (Exception ex) when (!(ex is DataException))
            {
                throw new DataException($"Error loading encrypted data from {filePath}: {ex.Message}", ex);
            }
        }

        public async Task<bool> SerializeAsync<T>(T data, string filePath) where T : class
        {
            try
            {
                // Check if passphrase is available for encryption
                string passphrase = null;
                lock (_passphraseLock)
                {
                    passphrase = _sessionPassphrase;
                }
                
                if (string.IsNullOrEmpty(passphrase))
                {
                    // No passphrase available - use base serializer directly (no encryption)
                    return await _baseSerializer.SerializeAsync(data, filePath);
                }

                // Serialize to JSON first
                var json = _baseSerializer.Serialize(data);
                
                // Encrypt the JSON (salt is now constant - no need to set)
                var encryptedData = await _encryptionService.EncryptAsync(json, GetCurrentPassphrase());
                
                // Wrap in container and save
                var container = new EncryptedContainer
                {
                    Data = encryptedData,
                    Version = AppConfig.Application.Version
                };
                
                return await _baseSerializer.SerializeAsync(container, filePath);
            }
            catch (Exception ex)
            {
                throw new DataException($"Error saving encrypted data to {filePath}: {ex.Message}", ex);
            }
        }

        public T Deserialize<T>(string content) where T : class, new()
        {
            // For in-memory operations, always use base serializer (no encryption)
            return _baseSerializer.Deserialize<T>(content);
        }

        public string Serialize<T>(T data) where T : class
        {
            // For in-memory operations, always use base serializer (no encryption)
            return _baseSerializer.Serialize(data);
        }

        // Thread-safe passphrase storage for the current session
        private string _sessionPassphrase;
        private readonly object _passphraseLock = new object();

        public void SetPassphrase(string passphrase)
        {
            lock (_passphraseLock)
            {
                _sessionPassphrase = passphrase;
            }
        }

        public void ClearPassphrase()
        {
            lock (_passphraseLock)
            {
                _sessionPassphrase = null;
            }
        }

        private string GetCurrentPassphrase()
        {
            lock (_passphraseLock)
            {
                if (string.IsNullOrEmpty(_sessionPassphrase))
                    throw new InvalidOperationException("No passphrase available for encryption/decryption");
                return _sessionPassphrase;
            }
        }
    }

    /// <summary>
    /// Container for encrypted data
    /// </summary>
    public class EncryptedContainer
    {
        public string Data { get; set; }
        public string Version { get; set; }
    }
}