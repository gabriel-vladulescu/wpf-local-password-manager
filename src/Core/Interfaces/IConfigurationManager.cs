using System;
using System.ComponentModel;
using AccountManager.Models;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Interface for managing application configuration and settings
    /// </summary>
    public interface IConfigurationManager : INotifyPropertyChanged
    {
        // Privacy & Security Settings
        bool CensorAccountData { get; set; }
        bool CensorPassword { get; set; }
        bool EnableEncryption { get; set; }

        // UI Settings
        bool EnableLocalSearch { get; set; }
        bool EnableApplicationNotifications { get; set; }

        // Confirmation Settings
        bool ConfirmAccountDelete { get; set; }
        bool ConfirmGroupDelete { get; set; }
        bool ConfirmArchiveAccount { get; set; }

        // Trash & Archive Settings
        bool EnableTrash { get; set; }
        bool EnableArchive { get; set; }
        bool ShowFavoritesGroup { get; set; }
        int TrashRetentionDays { get; set; }
        bool AutoEmptyTrash { get; set; }

        // Data File Settings
        string CustomDataPath { get; set; }

        // Events
        event Action<bool> TrashSettingChanged;
        event Action<bool> ArchiveSettingChanged;
        event Action<bool> FavoritesVisibilityChanged;

        // Methods
        void LoadSettings();
        void SaveSettings();
        void ResetToDefaults();
    }
}