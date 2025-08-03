using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Utilities.Helpers;
using AccountManager.Views.Base;
using AccountManager.ViewModels.Commands;

namespace AccountManager.ViewModels
{
    public class AccountDialogViewModel : BaseDialogViewModel
    {
        private Account _account = new();
        private Account _originalAccount;

        public Account Account
        {
            get => _account;
            set
            {
                if (_account != null)
                    _account.PropertyChanged -= Account_PropertyChanged;

                SetProperty(ref _account, value);

                if (_account != null)
                    _account.PropertyChanged += Account_PropertyChanged;

                ValidateData();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public override bool CanSave => !string.IsNullOrWhiteSpace(Account?.Name) && !IsBusy && !HasValidationError;

        public string Title => Mode switch
        {
            DialogMode.Edit => "Edit Account",
            DialogMode.View => "View Account",
            _ => "Create Account"
        };

        public string Subtitle => Mode switch
        {
            DialogMode.Edit => "Update your account information",
            DialogMode.View => "Account details",
            _ => "Add a new account to your collection"
        };

        public string ActionButtonText => Mode switch
        {
            DialogMode.Edit => "Update Account",
            DialogMode.View => "Close",
            _ => "Create Account"
        };

        public string IconKind => Mode switch
        {
            DialogMode.Edit => "AccountEdit",
            DialogMode.View => "Account",
            _ => "AccountPlus"
        };

        public ICommand GeneratePasswordCommand { get; }

        public AccountDialogViewModel()
        {
            GeneratePasswordCommand = new RelayCommand(GeneratePassword, () => !IsViewMode);
            Account = new Account();
        }

        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            Account = new Account();
            _originalAccount = null;
            ValidateData();
        }

        public void InitializeForEdit(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            Mode = DialogMode.Edit;
            _originalAccount = account;
            
            Account = account.Clone();
            ValidateData();
        }

        public void InitializeForView(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            Mode = DialogMode.View;
            _originalAccount = account;
            
            Account = account.Clone();
            ValidateData();
        }

        public void ApplyChanges()
        {
            if (IsEditMode && _originalAccount != null && CanSave)
            {
                _originalAccount.Name = Account.Name?.Trim();
                _originalAccount.Username = Account.Username?.Trim();
                _originalAccount.Email = Account.Email?.Trim();
                _originalAccount.Password = Account.Password;
                _originalAccount.Website = Account.Website?.Trim();
                _originalAccount.Notes = Account.Notes?.Trim();
            }
        }

        public Account CreateAccount()
        {
            if (IsCreateMode && CanSave)
            {
                return new Account
                {
                    Name = Account.Name?.Trim(),
                    Username = Account.Username?.Trim(),
                    Email = Account.Email?.Trim(),
                    Password = Account.Password,
                    Website = Account.Website?.Trim(),
                    Notes = Account.Notes?.Trim()
                };
            }
            return null;
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateData();
            if (e.PropertyName == nameof(Account.Name))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private void GeneratePassword()
        {
            if (IsViewMode) return;

            var options = PasswordGenerationOptions.Presets.Standard;
            Account.Password = PasswordGenerator.Generate(options);
        }

        protected override void ValidateData()
        {
            var validation = ValidationService.Instance.ValidateAccount(Account);
            ValidationError = validation.FirstError ?? "";
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(IconKind));
            CommandManager.InvalidateRequerySuggested();
        }
    }
}