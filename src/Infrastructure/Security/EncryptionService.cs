using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Exceptions;
using AccountManager.Config;

namespace AccountManager.Infrastructure.Security
{
    /// <summary>
    /// AES-256-GCM encryption service with PBKDF2 key derivation
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private const int KeySize = 32; // 256 bits
        private const int NonceSize = 12; // 96 bits for GCM
        private const int TagSize = 16; // 128 bits
        private const int SaltSize = 16; // 128 bits
        private const int Iterations = 100000; // PBKDF2 iterations

        // Salt is now constant from AppConfig

        // Salt generation removed - using constant salt from AppConfig

        public async Task<string> EncryptAsync(string data, string passphrase)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
            
            if (string.IsNullOrEmpty(passphrase))
                throw new ArgumentException("Passphrase cannot be null or empty", nameof(passphrase));

            return await Task.Run(() =>
            {
                try
                {
                    var dataBytes = Encoding.UTF8.GetBytes(data);
                    var saltBytes = Encoding.UTF8.GetBytes(AppConfig.Encryption.Salt);
                    
                    // Generate nonce
                    var nonce = new byte[NonceSize];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(nonce);
                    }
                    
                    // Derive key from passphrase and salt
                    var key = DeriveKey(passphrase, saltBytes);
                    
                    // Encrypt using AES-GCM
                    var ciphertext = new byte[dataBytes.Length];
                    var tag = new byte[TagSize];
                    
                    using (var aes = new AesGcm(key, TagSize))
                    {
                        aes.Encrypt(nonce, dataBytes, ciphertext, tag);
                    }
                    
                    // Combine nonce + ciphertext + tag
                    var result = new byte[NonceSize + ciphertext.Length + TagSize];
                    Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
                    Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
                    Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);
                    
                    // Clear sensitive data
                    Array.Clear(key, 0, key.Length);
                    
                    return Convert.ToBase64String(result);
                }
                catch (Exception ex)
                {
                    throw new DataException($"Encryption failed: {ex.Message}", ex);
                }
            });
        }

        public async Task<string> DecryptAsync(string encryptedData, string passphrase)
        {
            if (string.IsNullOrEmpty(encryptedData))
                throw new ArgumentException("Encrypted data cannot be null or empty", nameof(encryptedData));
            
            if (string.IsNullOrEmpty(passphrase))
                throw new ArgumentException("Passphrase cannot be null or empty", nameof(passphrase));

            return await Task.Run(() =>
            {
                try
                {
                    var encryptedBytes = Convert.FromBase64String(encryptedData);
                    var saltBytes = Encoding.UTF8.GetBytes(AppConfig.Encryption.Salt);
                    
                    if (encryptedBytes.Length < NonceSize + TagSize)
                        throw new DataException("Invalid encrypted data format");
                    
                    // Extract components
                    var nonce = new byte[NonceSize];
                    var tag = new byte[TagSize];
                    var ciphertext = new byte[encryptedBytes.Length - NonceSize - TagSize];
                    
                    Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
                    Buffer.BlockCopy(encryptedBytes, NonceSize, ciphertext, 0, ciphertext.Length);
                    Buffer.BlockCopy(encryptedBytes, NonceSize + ciphertext.Length, tag, 0, TagSize);
                    
                    // Derive key from passphrase and salt
                    var key = DeriveKey(passphrase, saltBytes);
                    
                    // Decrypt using AES-GCM
                    var plaintext = new byte[ciphertext.Length];
                    
                    using (var aes = new AesGcm(key, TagSize))
                    {
                        aes.Decrypt(nonce, ciphertext, tag, plaintext);
                    }
                    
                    // Clear sensitive data
                    Array.Clear(key, 0, key.Length);
                    
                    return Encoding.UTF8.GetString(plaintext);
                }
                catch (CryptographicException ex)
                {
                    throw new DataException("Decryption failed - invalid passphrase or corrupted data", ex);
                }
                catch (Exception ex)
                {
                    throw new DataException($"Decryption failed: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> ValidatePassphraseAsync(string testPayload, string passphrase)
        {
            try
            {
                await DecryptAsync(testPayload, passphrase);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] DeriveKey(string passphrase, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(passphrase, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }
    }
}