using System;
using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for encryption and decryption operations
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the specified data using the provided passphrase
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="passphrase">The passphrase to use for encryption</param>
        /// <returns>Base64 encoded encrypted data</returns>
        Task<string> EncryptAsync(string data, string passphrase);

        /// <summary>
        /// Decrypts the specified encrypted data using the provided passphrase
        /// </summary>
        /// <param name="encryptedData">Base64 encoded encrypted data</param>
        /// <param name="passphrase">The passphrase to use for decryption</param>
        /// <returns>The decrypted data</returns>
        Task<string> DecryptAsync(string encryptedData, string passphrase);

        /// <summary>
        /// Validates if the provided passphrase can decrypt the data (simplified validation)
        /// </summary>
        /// <param name="testPayload">Not used - kept for interface compatibility</param>
        /// <param name="passphrase">The passphrase to validate</param>
        /// <returns>True if passphrase is correct</returns>
        Task<bool> ValidatePassphraseAsync(string testPayload, string passphrase);
    }
}