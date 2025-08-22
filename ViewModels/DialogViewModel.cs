using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Utilities.Helpers;
using MaterialDesignThemes.Wpf;

namespace AccountManager.ViewModels
{
    public enum DialogMode
    {
        Create,
        Edit,
        View
    }

    public abstract class BaseDialogViewModel : BaseViewModel
    {
        private DialogMode _mode = DialogMode.Create;

        public DialogMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertiesChanged(nameof(IsEditMode), nameof(IsCreateMode), nameof(IsViewMode));
                    OnModeChanged();
                }
            }
        }

        public bool IsEditMode => Mode == DialogMode.Edit;
        public bool IsCreateMode => Mode == DialogMode.Create;
        public bool IsViewMode => Mode == DialogMode.View;

        protected virtual void OnModeChanged() {}
    }

    public class GroupDialogViewModel : BaseDialogViewModel
    {
        private string _groupName = "";
        private AccountGroup _originalGroup;
        private AccountGroup _tempGroup = new();

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (SetProperty(ref _groupName, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public AccountGroup TempGroup
        {
            get => _tempGroup;
            set => SetProperty(ref _tempGroup, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(GroupName);

        public string Title => IsEditMode ? "Edit Group" : "Create New Group";

        public string Subtitle =>
            IsEditMode ? "Update your group information" : "Organize your accounts with a new group";

        public string ActionButtonText => IsEditMode ? "Save Changes" : "Create Group";
        public string IconKind => IsEditMode ? "FolderEdit" : "FolderPlus";

        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            GroupName = "";
            _originalGroup = null;

            TempGroup = new AccountGroup
            {
                Name = "",
                ColorVariant = "#a779ff",
                Icon = "Folder"
            };
        }

        public void InitializeForEdit(AccountGroup group)
        {
            Mode = DialogMode.Edit;
            _originalGroup = group;
            GroupName = group.Name;

            TempGroup = new AccountGroup
            {
                Name = group.Name,
                ColorVariant = group.ColorVariant,
                Icon = group.Icon
            };
        }

        public void ApplyChanges()
        {
            if (IsEditMode && _originalGroup != null)
            {
                _originalGroup.Name = GroupName;
                _originalGroup.ColorVariant = TempGroup.ColorVariant;
                _originalGroup.Icon = TempGroup.Icon;
            }
        }

        public AccountGroup CreateGroup()
        {
            if (IsCreateMode)
            {
                return new AccountGroup
                {
                    Name = GroupName,
                    ColorVariant = TempGroup.ColorVariant,
                    Icon = TempGroup.Icon
                };
            }

            return null;
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertiesChanged(nameof(Title), nameof(Subtitle), nameof(ActionButtonText), nameof(IconKind));
        }
    }

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

                if (!SetProperty(ref _account, value)) return;
                
                if (_account != null)
                  _account.PropertyChanged += Account_PropertyChanged;

                OnPropertyChanged(nameof(CanSave));
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(Account?.Name);

        public string Title => IsEditMode ? "Edit Account" : "Create Account";

        public string Subtitle =>
            IsEditMode ? "Update your account information" : "Add a new account to your collection";

        public string ActionButtonText => IsEditMode ? "Update Account" : "Create Account";
        public string IconKind => IsEditMode ? "AccountEdit" : "AccountPlus";

        public ICommand GeneratePasswordCommand { get; }

        public AccountDialogViewModel()
        {
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
            Account = new Account();
        }

        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            Account = new Account();
            _originalAccount = null;
        }

        public void InitializeForEdit(Account account)
        {
            Mode = DialogMode.Edit;
            _originalAccount = account;

            Account = new Account
            {
                Name = account.Name,
                Username = account.Username,
                Email = account.Email,
                Password = account.Password,
                Website = account.Website,
                Notes = account.Notes
            };
        }

        public void ApplyChanges()
        {
            if (IsEditMode && _originalAccount != null)
            {
                _originalAccount.Name = Account.Name;
                _originalAccount.Username = Account.Username;
                _originalAccount.Email = Account.Email;
                _originalAccount.Password = Account.Password;
                _originalAccount.Website = Account.Website;
                _originalAccount.Notes = Account.Notes;
            }
        }

        public Account CreateAccount()
        {
            if (IsCreateMode)
            {
                return new Account
                {
                    Name = Account.Name,
                    Username = Account.Username,
                    Email = Account.Email,
                    Password = Account.Password,
                    Website = Account.Website,
                    Notes = Account.Notes
                };
            }

            return null;
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Account.Name))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private void GeneratePassword(object parameter)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            Account.Password = password;
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertiesChanged(nameof(Title), nameof(Subtitle), nameof(ActionButtonText), nameof(IconKind));
        }
    }
}