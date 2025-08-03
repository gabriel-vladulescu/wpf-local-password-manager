namespace AccountManager.Utilities.Constants
{
    public static class AppConstants
    {
        public const string ApplicationName = "Account Manager";
        public const string ApplicationVersion = "1.0.0";
        public const string DataFileName = "accounts.json";
        public const string SettingsFileName = "app-settings.json";
        public const string ThemeSettingsFileName = "settings.json";
        public const string BackupFileExtension = ".backup";
        
        public static class Validation
        {
            public const int MinAccountNameLength = 2;
            public const int MaxAccountNameLength = 100;
            public const int MinGroupNameLength = 2;
            public const int MaxGroupNameLength = 50;
            public const int MaxUsernameLength = 50;
            public const int MaxEmailLength = 254;
            public const int MaxWebsiteLength = 2048;
            public const int MaxPasswordLength = 128;
            public const int MinPasswordLength = 4;
        }

        public static class UI
        {
            public const int DefaultDialogWidth = 500;
            public const int DefaultDialogHeight = 400;
            public const int AutoSaveIntervalMinutes = 5;
            public const string DefaultCensoredText = "••••••••••••";
        }

        public static class Security
        {
            public const int DefaultPasswordLength = 16;
            public const int MaxPasswordLength = 128;
            public const int PasswordRetryAttempts = 3;
        }
    }
}