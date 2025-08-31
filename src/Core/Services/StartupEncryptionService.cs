using System;
using System.Threading.Tasks;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Views.Dialogs;
using AccountManager.ViewModels;

namespace AccountManager.Core.Services
{
    /// <summary>
    /// Handles encryption-related startup tasks like passphrase prompts
    /// </summary>
    public class StartupEncryptionService
    {
        private readonly IEncryptionConfigManager _encryptionConfigManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IDialogManager _dialogManager;
        private readonly INotificationService _notificationService;

        public StartupEncryptionService(
            IEncryptionConfigManager encryptionConfigManager,
            IEncryptionService encryptionService,
            IDialogManager dialogManager,
            INotificationService notificationService)
        {
            _encryptionConfigManager = encryptionConfigManager ?? throw new ArgumentNullException(nameof(encryptionConfigManager));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _dialogManager = dialogManager ?? throw new ArgumentNullException(nameof(dialogManager));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Checks if encryption is enabled and prompts for passphrase if needed
        /// </summary>
        /// <returns>True if encryption is ready or not needed, false if user cancelled</returns>
        public async Task<bool> InitializeEncryptionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("StartupEncryptionService: Checking if encryption is enabled");
                var isEncryptionEnabled = await _encryptionConfigManager.IsEncryptionEnabledAsync();
                System.Diagnostics.Debug.WriteLine($"StartupEncryptionService: Encryption enabled = {isEncryptionEnabled}");
                
                if (!isEncryptionEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("StartupEncryptionService: No encryption needed, returning true");
                    // No encryption needed
                    return true;
                }

                System.Diagnostics.Debug.WriteLine("StartupEncryptionService: Encryption enabled, prompting for passphrase");
                // Encryption is enabled - need to get passphrase
                return await PromptForPassphraseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartupEncryptionService: Exception = {ex}");
                _notificationService?.ShowError($"Error initializing encryption: {ex.Message}", "Encryption Error");
                return false;
            }
        }

        /// <summary>
        /// Shows passphrase dialog and validates the entered passphrase
        /// </summary>
        /// <returns>True if passphrase is correct, false if cancelled or incorrect</returns>
        private async Task<bool> PromptForPassphraseAsync()
        {
            const int maxAttempts = 3;
            int attempts = 0;

            System.Diagnostics.Debug.WriteLine("StartupEncryptionService: Starting passphrase prompt loop");

            while (attempts < maxAttempts)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"StartupEncryptionService: Passphrase attempt {attempts + 1}");
                    
                    var passphraseDialog = new PassphraseDialog();
                    passphraseDialog.SetupForExistingEncryption();

                    System.Diagnostics.Debug.WriteLine("StartupEncryptionService: Showing passphrase dialog");
                    var result = await _dialogManager.ShowDialogAsync(passphraseDialog);
                    System.Diagnostics.Debug.WriteLine($"StartupEncryptionService: Dialog result = {result}");
                    
                    if (result != true)
                    {
                        System.Diagnostics.Debug.WriteLine("StartupEncryptionService: User cancelled passphrase dialog");
                        // User cancelled
                        return false;
                    }

                    var enteredPassphrase = passphraseDialog.GetPassphrase();
                    if (string.IsNullOrEmpty(enteredPassphrase))
                    {
                        continue;
                    }

                    // Validate passphrase by trying to decrypt the test payload
                    if (await ValidatePassphraseAsync(enteredPassphrase))
                    {
                        // Set passphrase in the encrypted serializer for the session
                        var encryptedSerializer = ServiceContainer.Instance.GetEncryptedSerializer();
                        if (encryptedSerializer != null)
                        {
                            encryptedSerializer.SetPassphrase(enteredPassphrase);
                        }

                        passphraseDialog.ClearPasswords();
                        return true;
                    }
                    else
                    {
                        attempts++;
                        
                        if (attempts < maxAttempts)
                        {
                            _notificationService?.ShowError($"Incorrect passphrase. {maxAttempts - attempts} attempts remaining.", "Invalid Passphrase");
                        }
                        else
                        {
                            _notificationService?.ShowError("Maximum attempts reached. The application cannot access encrypted data.", "Access Denied");
                        }

                        passphraseDialog.ClearPasswords();
                    }
                }
                catch (Exception ex)
                {
                    _notificationService?.ShowError($"Error validating passphrase: {ex.Message}", "Encryption Error");
                    attempts++;
                }
            }

            return false;
        }

        /// <summary>
        /// Validates the passphrase by attempting to decrypt the actual data file
        /// </summary>
        /// <param name="passphrase">The passphrase to validate</param>
        /// <returns>True if passphrase is correct</returns>
        private async Task<bool> ValidatePassphraseAsync(string passphrase)
        {
            try
            {
                // Set passphrase temporarily in the encrypted serializer
                var encryptedSerializer = ServiceContainer.Instance.GetEncryptedSerializer();
                if (encryptedSerializer == null)
                {
                    return false;
                }

                // Set the passphrase
                encryptedSerializer.SetPassphrase(passphrase);

                try
                {
                    // Try to load the data - this will fail if passphrase is wrong
                    var dataRepository = ServiceContainer.Instance.DataRepository;
                    await dataRepository.GetAsync();
                    
                    // If we got here, passphrase is correct
                    return true;
                }
                catch
                {
                    // Passphrase is incorrect - clear it
                    encryptedSerializer.ClearPassphrase();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}