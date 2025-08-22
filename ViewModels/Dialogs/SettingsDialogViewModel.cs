using System.Windows.Input;
using AccountManager.Services;
using AccountManager.Utilities.Helpers;

namespace AccountManager.ViewModels
{
    public class SettingsDialogViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;

        // Privacy & Security Settings
        public bool CensorAccountData
        {
            get => _settingsService.CensorAccountData;
            set => _settingsService.CensorAccountData = value;
        }

        public bool CensorPassword
        {
            get => _settingsService.CensorPassword;
            set => _settingsService.CensorPassword = value;
        }

        public bool EnableEncryption
        {
            get => _settingsService.EnableEncryption;
            set => _settingsService.EnableEncryption = value;
        }

        // Data Management Settings
        public bool EnableTrash
        {
            get => _settingsService.EnableTrash;
            set => _settingsService.EnableTrash = value;
        }

        public bool EnableArchive
        {
            get => _settingsService.EnableArchive;
            set => _settingsService.EnableArchive = value;
        }

        public bool AutoEmptyTrash
        {
            get => _settingsService.AutoEmptyTrash;
            set => _settingsService.AutoEmptyTrash = value;
        }

        public int TrashRetentionDays
        {
            get => _settingsService.TrashRetentionDays;
            set => _settingsService.TrashRetentionDays = value;
        }

        // UI Settings
        public bool EnableLocalSearch
        {
            get => _settingsService.EnableLocalSearch;
            set => _settingsService.EnableLocalSearch = value;
        }

        // Confirmation Settings
        public bool ConfirmAccountDelete
        {
            get => _settingsService.ConfirmAccountDelete;
            set => _settingsService.ConfirmAccountDelete = value;
        }

        public bool ConfirmGroupDelete
        {
            get => _settingsService.ConfirmGroupDelete;
            set => _settingsService.ConfirmGroupDelete = value;
        }

        // Computed properties
        public bool ShowTrashRetention => EnableTrash && AutoEmptyTrash;

        // Commands
        public ICommand ResetToDefaultsCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public SettingsDialogViewModel()
        {
            _settingsService = SettingsService.Instance;
            InitializeCommands();
            
            // Subscribe to settings changes to update computed properties
            _settingsService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EnableTrash) || e.PropertyName == nameof(AutoEmptyTrash))
                {
                    OnPropertyChanged(nameof(ShowTrashRetention));
                }
            };
        }

        private void InitializeCommands()
        {
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            CloseCommand = new RelayCommand(Close);
        }

        public void InitializeForView()
        {
            // Refresh all property bindings
            OnPropertyChanged(string.Empty);
        }

        private void ResetToDefaults(object parameter)
        {
            _settingsService.ResetToDefaults();
            
            // Refresh all bindings
            OnPropertyChanged(string.Empty);
        }

        private void Close(object parameter)
        {
            // Dialog will be closed by the dialog service
        }
    }
}