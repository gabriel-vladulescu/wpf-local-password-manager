using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class PassphraseDialogViewModel : INotifyPropertyChanged
    {
        private readonly IEncryptionService _encryptionService;
        private readonly INotificationService _notificationService;
        private string _passphrase = "";
        private string _confirmPassphrase = "";
        private bool _isNewEncryption = true;
        private bool _showConfirmation = true;
        private string _validationError = "";

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> CloseRequested;

        public PassphraseDialogViewModel()
        {
            System.Diagnostics.Debug.WriteLine("PassphraseDialogViewModel: Constructor called");
            try
            {
                _encryptionService = ServiceContainer.Instance.EncryptionService;
                _notificationService = ServiceContainer.Instance.NotificationService;
                
                System.Diagnostics.Debug.WriteLine("PassphraseDialogViewModel: Services resolved");
                
                InitializeCommands();
                System.Diagnostics.Debug.WriteLine("PassphraseDialogViewModel: Commands initialized");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PassphraseDialogViewModel: Constructor exception: {ex}");
                throw;
            }
        }

        // Properties
        public string Title => _isNewEncryption ? "Set Encryption Passphrase" : "Enter Encryption Passphrase";
        public string Message => _isNewEncryption 
            ? "Create a strong passphrase to encrypt your accounts file. This passphrase will be required each time you start the application."
            : "Enter your passphrase to decrypt the accounts file.";

        public string Passphrase
        {
            get => _passphrase;
            set
            {
                _passphrase = value;
                OnPropertyChanged();
                ValidatePassphrase();
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        public string ConfirmPassphrase
        {
            get => _confirmPassphrase;
            set
            {
                _confirmPassphrase = value;
                OnPropertyChanged();
                ValidatePassphrase();
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        public bool ShowConfirmation
        {
            get => _showConfirmation;
            set
            {
                _showConfirmation = value;
                OnPropertyChanged();
            }
        }

        public string ValidationError
        {
            get => _validationError;
            set
            {
                _validationError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationError));
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(_validationError);

        public bool IsNewEncryption
        {
            get => _isNewEncryption;
            set
            {
                _isNewEncryption = value;
                _showConfirmation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(ShowConfirmation));
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

        public bool CanConfirm
        {
            get
            {
                if (string.IsNullOrEmpty(_passphrase))
                    return false;

                if (_isNewEncryption)
                {
                    return _passphrase.Length >= 6 && 
                           _passphrase == _confirmPassphrase &&
                           string.IsNullOrEmpty(_validationError);
                }

                return !string.IsNullOrEmpty(_passphrase);
            }
        }

        // Commands
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void InitializeCommands()
        {
            ConfirmCommand = new RelayCommand(Confirm, _ => CanConfirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void SetupForNewEncryption()
        {
            IsNewEncryption = true;
            _passphrase = "";
            _confirmPassphrase = "";
            _validationError = "";
            OnPropertyChanged(nameof(Passphrase));
            OnPropertyChanged(nameof(ConfirmPassphrase));
            OnPropertyChanged(nameof(ValidationError));
        }

        public void SetupForExistingEncryption()
        {
            System.Diagnostics.Debug.WriteLine("PassphraseDialogViewModel: SetupForExistingEncryption called");
            try
            {
                IsNewEncryption = false;
                _passphrase = "";
                _validationError = "";
                OnPropertyChanged(nameof(Passphrase));
                OnPropertyChanged(nameof(ValidationError));
                System.Diagnostics.Debug.WriteLine("PassphraseDialogViewModel: SetupForExistingEncryption completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PassphraseDialogViewModel: SetupForExistingEncryption exception: {ex}");
            }
        }

        private void ValidatePassphrase()
        {
            if (!_isNewEncryption)
            {
                ValidationError = "";
                return;
            }

            if (string.IsNullOrEmpty(_passphrase))
            {
                ValidationError = "";
                return;
            }

            if (_passphrase.Length < 6)
            {
                ValidationError = "Passphrase must be at least 6 characters long";
                return;
            }

            if (_showConfirmation && !string.IsNullOrEmpty(_confirmPassphrase) && _passphrase != _confirmPassphrase)
            {
                ValidationError = "Passphrases do not match";
                return;
            }

            ValidationError = "";
        }

        private void Confirm(object parameter)
        {
            if (!CanConfirm)
                return;

            try
            {
                // For new encryption, we'll handle the encryption setup in the calling code
                // For existing encryption, we'll validate the passphrase in the calling code
                
                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                _notificationService?.ShowError($"Error: {ex.Message}", "Passphrase Error");
            }
        }

        private void Cancel(object parameter)
        {
            CloseRequested?.Invoke(this, false);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}