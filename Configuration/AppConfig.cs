using System;
using System.Configuration;
using AccountManager.Utilities.Constants;

namespace AccountManager.Configuration
{
    public static class AppConfig
    {
        public static class Database
        {
            public static string DataFilePath => GetSetting("DataFilePath", AppConstants.DataFileName);
            public static string SettingsFilePath => GetSetting("SettingsFilePath", AppConstants.SettingsFileName);
            public static bool EnableAutoBackup => GetBoolSetting("EnableAutoBackup", true);
            public static int MaxBackupFiles => GetIntSetting("MaxBackupFiles", 10);
            public static TimeSpan BackupInterval => TimeSpan.FromMinutes(GetIntSetting("BackupIntervalMinutes", 30));
        }

        public static class Security
        {
            public static bool EnableEncryption => GetBoolSetting("EnableEncryption", false);
            public static string EncryptionAlgorithm => GetSetting("EncryptionAlgorithm", "AES-256");
            public static int DefaultPasswordLength => GetIntSetting("DefaultPasswordLength", AppConstants.Security.DefaultPasswordLength);
            public static int MaxPasswordLength => GetIntSetting("MaxPasswordLength", AppConstants.Security.MaxPasswordLength);
            public static int LoginAttempts => GetIntSetting("MaxLoginAttempts", AppConstants.Security.PasswordRetryAttempts);
        }

        public static class UI
        {
            public static int AutoSaveInterval => GetIntSetting("AutoSaveIntervalMinutes", AppConstants.UI.AutoSaveIntervalMinutes);
            public static bool ShowWelcomeScreen => GetBoolSetting("ShowWelcomeScreen", true);
            public static bool MinimizeToSystemTray => GetBoolSetting("MinimizeToSystemTray", false);
            public static string DefaultTheme => GetSetting("DefaultTheme", "Light");
            public static bool EnableAnimations => GetBoolSetting("EnableAnimations", true);
        }

        public static class Logging
        {
            public static bool EnableLogging => GetBoolSetting("EnableLogging", true);
            public static string LogLevel => GetSetting("LogLevel", "Information");
            public static string LogFilePath => GetSetting("LogFilePath", "logs/app.log");
            public static int MaxLogFileSizeMB => GetIntSetting("MaxLogFileSizeMB", 10);
            public static int MaxLogFiles => GetIntSetting("MaxLogFiles", 5);
        }

        public static class Performance
        {
            public static int SearchThrottleMs => GetIntSetting("SearchThrottleMs", 300);
            public static int MaxRecentItems => GetIntSetting("MaxRecentItems", 10);
            public static bool EnableLazyLoading => GetBoolSetting("EnableLazyLoading", true);
            public static int VirtualizationThreshold => GetIntSetting("VirtualizationThreshold", 100);
        }

        private static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                return ConfigurationManager.AppSettings[key] ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool GetBoolSetting(string key, bool defaultValue = false)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return bool.TryParse(value, out var result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static int GetIntSetting(string key, int defaultValue = 0)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return int.TryParse(value, out var result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}