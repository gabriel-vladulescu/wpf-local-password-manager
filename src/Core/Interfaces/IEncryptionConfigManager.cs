using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Manages encryption detection and configuration using constant salt from AppConfig
    /// </summary>
    public interface IEncryptionConfigManager
    {
        /// <summary>
        /// Checks if encryption is currently enabled by examining file content
        /// </summary>
        Task<bool> IsEncryptionEnabledAsync();

        /// <summary>
        /// Gets the constant encryption salt from AppConfig
        /// </summary>
        Task<string> GetSaltAsync();

        /// <summary>
        /// Gets the test payload for passphrase validation (returns null - not used)
        /// </summary>
        Task<string> GetTestPayloadAsync();
    }
}