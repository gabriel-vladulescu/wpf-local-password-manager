using System;

namespace AccountManager.Config
{
    public static class AppConfig
    {
        // Application Information
        public static class Application
        {
            public const string Name = "PassVault";
            public const string Version = "2.2.0";
            public const string Company = "Avanty.Works";
            public const string Description = "Secure password and account management application";
            public const string Copyright = "Copyright © 2025";
        }

        // File and Storage
        public static class Storage
        {
            public const string DataFileName = "accounts.json";
            public const string BackupExtension = ".backup";
        }

        // Asset Paths
        public static class Assets
        {
            public const string LogoLight = "pack://application:,,,/src/Assets/logo-light.png";
            public const string LogoDark = "pack://application:,,,/src/Assets/logo-dark.png";
            public const string LogoDefault = "pack://application:,,,/src/Assets/logo.png";
            public const string AppIcon = "src/Assets/icon.ico";
        }

        // Default Settings
        public static class Defaults
        {
            public const bool CensorAccountData = false;
            public const bool CensorPassword = true;
            public const bool EnableEncryption = false;
            public const bool EnableLocalSearch = false;
            public const bool EnableApplicationNotifications = true;
            public const bool ConfirmAccountDelete = true;
            public const bool ConfirmGroupDelete = true;
            public const bool ConfirmArchiveAccount = true;
            public const bool EnableTrash = true;
            public const bool EnableArchive = true;
            public const int TrashRetentionDays = 30;
            public const bool AutoEmptyTrash = false;
            public const bool ShowFavoritesGroup = true;
        }

        // Encryption Settings
        public static class Encryption
        {
            public const string Salt = "PassVault2024_SecureSalt_v1.0.0"; // Fixed salt for the application
        }

        // Theme Settings
        public static class Theme
        {
            public const string DefaultTheme = "Light";
            public const string LightThemeName = "Light";
            public const string DarkThemeName = "Dark";
        }

        // UI Constants
        public static class UI
        {
            public const int MinWindowWidth = 800;
            public const int MinWindowHeight = 600;
            public const int DefaultWindowWidth = 1200;
            public const int DefaultWindowHeight = 800;
            
            // Animation durations (milliseconds)
            public const int ShortAnimationDuration = 200;
            public const int MediumAnimationDuration = 300;
            public const int LongAnimationDuration = 500;
            
            // Search settings
            public const int SearchMinChars = 2;
            public const int SearchDelayMs = 300;
        }

        // Password Generation
        public static class PasswordGeneration
        {
            public const int DefaultLength = 16;
            public const int MinLength = 8;
            public const int MaxLength = 128;
            public const bool IncludeUppercase = true;
            public const bool IncludeLowercase = true;
            public const bool IncludeNumbers = true;
            public const bool IncludeSymbols = true;
            public const string SymbolCharacters = "!@#$%^&*";
        }

        // System Groups
        public static class SystemGroups
        {
            public const string AllItemsId = "all";
            public const string FavoritesId = "favorites";
            public const string TrashId = "trash";
            public const string ArchiveId = "archive";
            
            public const string AllItemsName = "All items";
            public const string FavoritesName = "Favorites";
            public const string TrashName = "Trash";
            public const string ArchiveName = "Archive";
            
            public const string AllItemsIcon = "ShieldKeyOutline";
            public const string FavoritesIcon = "StarOutline";
            public const string TrashIcon = "TrashCanOutline";
            public const string ArchiveIcon = "ArchiveOutline";
        }

        // Group Colors (default options)
        public static class GroupColors
        {
            public static readonly string[] DefaultColors = {
                "#6366F1", // Indigo
                "#8B5CF6", // Violet  
                "#EC4899", // Pink
                "#EF4444", // Red
                "#F59E0B", // Amber
                "#10B981", // Emerald
                "#06B6D4", // Cyan
                "#3B82F6", // Blue
                "#8B5A2B", // Brown
                "#6B7280"  // Gray
            };
        }

        // Group Icons (default options)
        public static class GroupIcons
        {
            public static readonly string[] DefaultIcons = {
                "Folder",
                "FolderOutline", 
                "UserMultipleOutline",
                "ShieldKeyOutline",
                "BriefcaseOutline",
                "HomeOutline",
                "CogOutline",
                "GamepadSquareOutline",
                "CreditCardOutline",
                "EmailOutline"
            };
        }
    }
}