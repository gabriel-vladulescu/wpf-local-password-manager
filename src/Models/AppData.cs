using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AccountManager.Config;

namespace AccountManager.Models
{
    public class AppData
    {
        public List<AccountGroup> Groups { get; set; } = new();
        public DateTime LastBackup { get; set; } = DateTime.MinValue;
        public string Version { get; set; } = AppConfig.Application.Version;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public AppSettings Settings { get; set; } = new();
        public ThemeSettings Theme { get; set; } = new();

        // Computed properties for analytics/UI
        [JsonIgnore]
        public int TotalGroups => Groups?.Count ?? 0;

        [JsonIgnore]
        public int TotalAccounts => Groups?.Sum(g => g.AccountCount) ?? 0;

        [JsonIgnore]
        public int TotalFavorites => Groups?.Sum(g => g.FavoriteCount) ?? 0;

        [JsonIgnore]
        public bool HasData => TotalGroups > 0;

        [JsonIgnore]
        public bool IsEmpty => !HasData;

        [JsonIgnore]
        public bool HasFavorites => TotalFavorites > 0;

        [JsonIgnore]
        public AccountGroup LargestGroup => Groups?.OrderByDescending(g => g.AccountCount).FirstOrDefault();

        [JsonIgnore]
        public AccountGroup GroupWithMostFavorites => Groups?.OrderByDescending(g => g.FavoriteCount).FirstOrDefault();

        [JsonIgnore]
        public Account MostRecentAccount => Groups?
            .SelectMany(g => g.Accounts)
            .OrderByDescending(a => a.LastModified)
            .FirstOrDefault();

        [JsonIgnore]
        public Account MostRecentFavorite => Groups?
            .SelectMany(g => g.Accounts)
            .Where(a => a.IsFavorite)
            .OrderByDescending(a => a.LastModified)
            .FirstOrDefault();

        [JsonIgnore]
        public string StatsText => $"{TotalGroups} groups, {TotalAccounts} accounts";

        [JsonIgnore]
        public string DetailedStatsText
        {
            get
            {
                if (TotalFavorites == 0)
                    return StatsText;
                return $"{StatsText}, {TotalFavorites} favorites";
            }
        }

        [JsonIgnore]
        public string LastBackupFormatted => LastBackup == DateTime.MinValue 
            ? "Never" 
            : LastBackup.ToString("MMM dd, yyyy 'at' HH:mm");

        [JsonIgnore]
        public List<Account> AllAccounts => Groups?.SelectMany(g => g.Accounts).ToList() ?? new List<Account>();

        [JsonIgnore]
        public List<Account> AllFavorites => Groups?
            .SelectMany(g => g.Accounts)
            .Where(a => a.IsFavorite)
            .OrderBy(a => a.Name)
            .ToList() ?? new List<Account>();

        public AppData()
        {
            Groups = new List<AccountGroup>();
            CreatedAt = DateTime.Now;
            Version = AppConfig.Application.Version;
            Settings = new AppSettings();
            Theme = new ThemeSettings();
        }

        public AccountGroup FindGroup(string groupName)
        {
            return Groups?.FirstOrDefault(g => string.Equals(g.Name, groupName, StringComparison.OrdinalIgnoreCase));
        }

        public bool ContainsGroup(string groupName)
        {
            return FindGroup(groupName) != null;
        }

        public void AddGroup(AccountGroup group)
        {
            if (group != null && !Groups.Contains(group))
            {
                Groups.Add(group);
            }
        }

        public void RemoveGroup(AccountGroup group)
        {
            if (group != null && Groups.Contains(group))
            {
                Groups.Remove(group);
            }
        }

        public List<Account> SearchAccounts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Account>();

            var search = searchTerm.ToLower();
            return Groups
                .SelectMany(g => g.Accounts)
                .Where(a => 
                    (a.Name?.ToLower().Contains(search) ?? false) ||
                    (a.Username?.ToLower().Contains(search) ?? false) ||
                    (a.Email?.ToLower().Contains(search) ?? false) ||
                    (a.Website?.ToLower().Contains(search) ?? false))
                .OrderBy(a => a.Name)
                .ToList();
        }

        public List<Account> SearchFavorites(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return AllFavorites;

            var search = searchTerm.ToLower();
            return AllFavorites
                .Where(a => 
                    (a.Name?.ToLower().Contains(search) ?? false) ||
                    (a.Username?.ToLower().Contains(search) ?? false) ||
                    (a.Email?.ToLower().Contains(search) ?? false) ||
                    (a.Website?.ToLower().Contains(search) ?? false))
                .OrderBy(a => a.Name)
                .ToList();
        }

        public Account FindAccountByName(string accountName)
        {
            return AllAccounts.FirstOrDefault(a => 
                string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));
        }

        public AccountGroup FindGroupContaining(Account account)
        {
            return Groups?.FirstOrDefault(g => g.Accounts.Contains(account));
        }

        public void UpdateBackupTime()
        {
            LastBackup = DateTime.Now;
        }

        public bool IsValidFormat()
        {
            return Groups != null && !string.IsNullOrWhiteSpace(Version);
        }

        public AppData Clone()
        {
            var clone = new AppData
            {
                LastBackup = this.LastBackup,
                Version = this.Version,
                CreatedAt = this.CreatedAt
            };

            foreach (var group in this.Groups)
            {
                clone.Groups.Add(group.Clone());
            }

            clone.Settings = new AppSettings
            {
                CensorAccountData = this.Settings.CensorAccountData,
                CensorPassword = this.Settings.CensorPassword,
                EnableEncryption = this.Settings.EnableEncryption,
                EnableLocalSearch = this.Settings.EnableLocalSearch,
                EnableApplicationNotifications = this.Settings.EnableApplicationNotifications,
                ConfirmAccountDelete = this.Settings.ConfirmAccountDelete,
                ConfirmGroupDelete = this.Settings.ConfirmGroupDelete,
                ConfirmArchiveAccount = this.Settings.ConfirmArchiveAccount,
                EnableTrash = this.Settings.EnableTrash,
                EnableArchive = this.Settings.EnableArchive,
                TrashRetentionDays = this.Settings.TrashRetentionDays,
                AutoEmptyTrash = this.Settings.AutoEmptyTrash,
                ShowFavoritesGroup = this.Settings.ShowFavoritesGroup,
                CustomDataPath = this.Settings.CustomDataPath
            };

            clone.Theme = new ThemeSettings
            {
                CurrentTheme = this.Theme.CurrentTheme
            };

            return clone;
        }

        public void Validate()
        {
            Settings ??= new AppSettings();
            Theme ??= new ThemeSettings();
            if (Groups == null)
                throw new InvalidOperationException("Groups collection cannot be null");

            if (string.IsNullOrWhiteSpace(Version))
                throw new InvalidOperationException("Version cannot be empty");

            // Remove any invalid groups
            Groups.RemoveAll(g => !g.IsValid());

            // Reinitialize event handlers for all groups after JSON deserialization
            foreach (var group in Groups)
            {
                group.ReinitializeAfterDeserialization();
            }

            // Remove any invalid accounts from groups
            foreach (var group in Groups)
            {
                var invalidAccounts = group.Accounts.Where(a => !a.IsValid()).ToList();
                foreach (var account in invalidAccounts)
                {
                    group.Accounts.Remove(account);
                }
            }
        }

        public Dictionary<string, object> GetAnalytics()
        {
            return new Dictionary<string, object>
            {
                ["totalGroups"] = TotalGroups,
                ["totalAccounts"] = TotalAccounts,
                ["totalFavorites"] = TotalFavorites,
                ["favoritePercentage"] = TotalAccounts > 0 ? Math.Round((double)TotalFavorites / TotalAccounts * 100, 1) : 0,
                ["averageAccountsPerGroup"] = TotalGroups > 0 ? Math.Round((double)TotalAccounts / TotalGroups, 1) : 0,
                ["largestGroupName"] = LargestGroup?.Name ?? "None",
                ["largestGroupSize"] = LargestGroup?.AccountCount ?? 0,
                ["groupWithMostFavoritesName"] = GroupWithMostFavorites?.Name ?? "None",
                ["groupWithMostFavoritesCount"] = GroupWithMostFavorites?.FavoriteCount ?? 0,
                ["lastActivity"] = MostRecentAccount?.LastModifiedFormatted ?? "Never",
                ["lastFavoriteActivity"] = MostRecentFavorite?.LastModifiedFormatted ?? "Never"
            };
        }
    }

    // App Settings class for JSON serialization
    public class AppSettings
    {
        public bool CensorAccountData { get; set; } = AppConfig.Defaults.CensorAccountData;
        public bool CensorPassword { get; set; } = AppConfig.Defaults.CensorPassword;
        public bool EnableEncryption { get; set; } = AppConfig.Defaults.EnableEncryption;
        public bool EnableLocalSearch { get; set; } = AppConfig.Defaults.EnableLocalSearch;
        public bool EnableApplicationNotifications { get; set; } = AppConfig.Defaults.EnableApplicationNotifications;
        public bool ConfirmAccountDelete { get; set; } = AppConfig.Defaults.ConfirmAccountDelete;
        public bool ConfirmGroupDelete { get; set; } = AppConfig.Defaults.ConfirmGroupDelete;
        public bool ConfirmArchiveAccount { get; set; } = AppConfig.Defaults.ConfirmArchiveAccount;
        public bool EnableTrash { get; set; } = AppConfig.Defaults.EnableTrash;
        public bool EnableArchive { get; set; } = AppConfig.Defaults.EnableArchive;
        public int TrashRetentionDays { get; set; } = AppConfig.Defaults.TrashRetentionDays;
        public bool AutoEmptyTrash { get; set; } = AppConfig.Defaults.AutoEmptyTrash;
        public bool ShowFavoritesGroup { get; set; } = AppConfig.Defaults.ShowFavoritesGroup;
        public string CustomDataPath { get; set; }
    }

    // Theme Settings class for JSON serialization
    public class ThemeSettings
    {
        public string CurrentTheme { get; set; } = AppConfig.Theme.DefaultTheme;
    }
}