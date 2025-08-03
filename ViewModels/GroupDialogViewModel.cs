using System;
using AccountManager.Models;
using AccountManager.Services;
using AccountManager.Views.Base;

namespace AccountManager.ViewModels
{
    public class GroupDialogViewModel : BaseDialogViewModel
    {
        private string _groupName = "";
        private AccountGroup _originalGroup;

        public string GroupName
        {
            get => _groupName;
            set
            {
                SetProperty(ref _groupName, value);
                ValidateData();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public override bool CanSave => !string.IsNullOrWhiteSpace(GroupName) && !IsBusy && !HasValidationError;

        public string Title => Mode switch
        {
            DialogMode.Edit => "Edit Group",
            DialogMode.View => "View Group",
            _ => "Create New Group"
        };

        public string Subtitle => Mode switch
        {
            DialogMode.Edit => "Update your group information",
            DialogMode.View => "Group details",
            _ => "Organize your accounts with a new group"
        };

        public string ActionButtonText => Mode switch
        {
            DialogMode.Edit => "Save Changes",
            DialogMode.View => "Close",
            _ => "Create Group"
        };

        public string IconKind => Mode switch
        {
            DialogMode.Edit => "FolderEdit",
            DialogMode.View => "Folder",
            _ => "FolderPlus"
        };

        public void InitializeForCreate()
        {
            Mode = DialogMode.Create;
            GroupName = "";
            _originalGroup = null;
            ValidateData();
        }

        public void InitializeForEdit(AccountGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            Mode = DialogMode.Edit;
            _originalGroup = group;
            GroupName = group.Name;
            ValidateData();
        }

        public void InitializeForView(AccountGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            Mode = DialogMode.View;
            _originalGroup = group;
            GroupName = group.Name;
            ValidateData();
        }

        public void ApplyChanges()
        {
            if (IsEditMode && _originalGroup != null && CanSave)
            {
                _originalGroup.Name = GroupName.Trim();
            }
        }

        public AccountGroup CreateGroup()
        {
            if (IsCreateMode && CanSave)
            {
                return new AccountGroup { Name = GroupName.Trim() };
            }
            return null;
        }

        protected override void ValidateData()
        {
            var validation = ValidationService.Instance.ValidateGroupName(GroupName);
            ValidationError = validation.FirstError ?? "";
        }

        protected override void OnModeChanged()
        {
            base.OnModeChanged();
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Subtitle));
            OnPropertyChanged(nameof(ActionButtonText));
            OnPropertyChanged(nameof(IconKind));
        }
    }
}