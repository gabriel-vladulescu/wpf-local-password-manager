using AccountManager.Services;
using AccountManager.Views.Base;

namespace AccountManager.ViewModels
{
    public class SettingsDialogViewModel : BaseDialogViewModel
    {
        private readonly SettingsService _settingsService;

        public SettingsDialogViewModel()
        {
            _settingsService = SettingsService.Instance;
        }

        public override bool CanSave => true; // Settings can always be saved

        public bool CensorAccountData
        {
            get => _settingsService.CensorAccountData;
            set
            {
                if (_settingsService.CensorAccountData != value)
                {
                    _settingsService.CensorAccountData = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CensorPassword
        {
            get => _settingsService.CensorPassword;
            set
            {
                if (_settingsService.CensorPassword != value)
                {
                    _settingsService.CensorPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableEncryption
        {
            get => _settingsService.EnableEncryption;
            set
            {
                if (_settingsService.EnableEncryption != value)
                {
                    _settingsService.EnableEncryption = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableLocalSearch
        {
            get => _settingsService.EnableLocalSearch;
            set
            {
                if (_settingsService.EnableLocalSearch != value)
                {
                    _settingsService.EnableLocalSearch = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmGroupDelete
        {
            get => _settingsService.ConfirmGroupDelete;
            set
            {
                if (_settingsService.ConfirmGroupDelete != value)
                {
                    _settingsService.ConfirmGroupDelete = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmAccountDelete
        {
            get => _settingsService.ConfirmAccountDelete;
            set
            {
                if (_settingsService.ConfirmAccountDelete != value)
                {
                    _settingsService.ConfirmAccountDelete = value;
                    OnPropertyChanged();
                }
            }
        }

        public void InitializeForView()
        {
            Mode = DialogMode.View;
            RefreshAllSettings();
        }

        private void RefreshAllSettings()
        {
            OnPropertyChanged(nameof(CensorAccountData));
            OnPropertyChanged(nameof(CensorPassword));
            OnPropertyChanged(nameof(EnableEncryption));
            OnPropertyChanged(nameof(EnableLocalSearch));
            OnPropertyChanged(nameof(ConfirmGroupDelete));
            OnPropertyChanged(nameof(ConfirmAccountDelete));
        }

        protected override void ValidateData()
        {
            // Settings don't need validation - they're always valid
            ValidationError = "";
        }
    }
}