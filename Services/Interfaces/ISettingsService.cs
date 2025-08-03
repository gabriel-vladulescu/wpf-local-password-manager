using System.ComponentModel;

namespace AccountManager.Services.Interfaces
{
    public interface ISettingsService : INotifyPropertyChanged
    {
        bool CensorAccountData { get; set; }
        bool CensorPassword { get; set; }
        bool EnableEncryption { get; set; }
        bool EnableLocalSearch { get; set; }
        bool ConfirmGroupDelete { get; set; }
        bool ConfirmAccountDelete { get; set; }
        bool ConfirmAccountEdit { get; set; }
        bool AutoSave { get; set; }
        int AutoSaveInterval { get; set; }

        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
    }
}