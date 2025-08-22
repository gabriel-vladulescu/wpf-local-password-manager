using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AccountManager.Models
{
    public class AccountGroup : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();

        private string _name = "";
        private string _icon = "Folder";
        private string _colorVariant = "#6366F1";
        private int _position = -1; // -1 = last, 0 = first, 1 = second, etc.
        private bool _isDefault = false;
        private ObservableCollection<Account> _accounts = new();
        private DateTime _createdAt = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        public string Icon
        {
            get => _icon;
            set { SetProperty(ref _icon, value); }
        }

        public string ColorVariant
        {
            get => _colorVariant;
            set { SetProperty(ref _colorVariant, value); }
        }

        public int Position
        {
            get => _position;
            set { SetProperty(ref _position, value); }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set { SetProperty(ref _isDefault, value); }
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set 
            { 
                if (_accounts != null)
                {
                    _accounts.CollectionChanged -= Accounts_CollectionChanged;
                    // Unsubscribe from individual account property changes
                    foreach (var account in _accounts)
                    {
                        account.PropertyChanged -= Account_PropertyChanged;
                    }
                }
                    
                SetProperty(ref _accounts, value);
                
                if (_accounts != null)
                {
                    _accounts.CollectionChanged += Accounts_CollectionChanged;
                    // Subscribe to individual account property changes
                    foreach (var account in _accounts)
                    {
                        account.PropertyChanged += Account_PropertyChanged;
                    }
                }
                    
                RefreshComputedProperties();
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { SetProperty(ref _createdAt, value); }
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set { SetProperty(ref _lastModified, value); }
        }

        // Computed properties for UI
        [JsonIgnore]
        public int AccountCount => Accounts?.Count ?? 0;

        [JsonIgnore]
        public int FavoriteCount => Accounts?.Count(a => a.IsFavorite) ?? 0;

        [JsonIgnore]
        public string AccountCountText => AccountCount == 1 ? "1 account" : $"{AccountCount} accounts";

        [JsonIgnore]
        public string FavoriteCountText => FavoriteCount == 1 ? "1 favorite" : $"{FavoriteCount} favorites";

        [JsonIgnore]
        public bool HasAccounts => AccountCount > 0;

        [JsonIgnore]
        public bool HasFavorites => FavoriteCount > 0;

        [JsonIgnore]
        public bool IsEmpty => !HasAccounts;

        [JsonIgnore]
        public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");

        [JsonIgnore]
        public string LastModifiedFormatted => LastModified.ToString("MMM dd, yyyy 'at' HH:mm");

        [JsonIgnore]
        public Account MostRecentAccount => Accounts?.OrderByDescending(a => a.LastModified).FirstOrDefault();

        [JsonIgnore]
        public string LastActivityText => MostRecentAccount != null 
            ? $"Last updated {MostRecentAccount.LastModifiedFormatted}"
            : "No recent activity";

        [JsonIgnore]
        public ObservableCollection<Account> FavoriteAccounts => 
            new ObservableCollection<Account>(Accounts?.Where(a => a.IsFavorite) ?? Enumerable.Empty<Account>());

        [JsonIgnore]
        public string GroupStatsText
        {
            get
            {
                if (AccountCount == 0) return "Empty group";
                if (FavoriteCount == 0) return AccountCountText;
                return $"{AccountCountText}, {FavoriteCountText}";
            }
        }

        [JsonIgnore]
        public bool CanEdit => !IsDefault;

        [JsonIgnore]
        public bool CanDelete => !IsDefault;

        [JsonIgnore]
        public string IconBackgroundColor => ColorVariant;

        [JsonIgnore]
        public string IconBackgroundColorWithOpacity 
        {
            get
            {
                // Convert hex to color with 10% opacity
                if (string.IsNullOrEmpty(ColorVariant) || !ColorVariant.StartsWith("#")) return "#10FFFFFF";
                
                var hex = ColorVariant.TrimStart('#');
                if (hex.Length == 6)
                {
                    return $"#1A{hex}"; // 1A is approximately 10% opacity
                }
                return "#10FFFFFF";
            }
        }

        [JsonIgnore]
        public bool IsFavoritesGroup => IsDefault && Name == "Favorites";

        public AccountGroup()
        {
            var now = DateTime.Now;
            _id = Guid.NewGuid().ToString();
            _createdAt = now;
            _lastModified = now;
            _accounts = new ObservableCollection<Account>();
            _accounts.CollectionChanged += Accounts_CollectionChanged;
        }

        public AccountGroup(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Creates the default Favorites group
        /// </summary>
        public static AccountGroup CreateFavoritesGroup()
        {
            return new AccountGroup
            {
                Name = "Favorites",
                Icon = "Star",
                ColorVariant = "#f7d775", // Gold/Yellow color for favorites
                Position = 0, // Always first
                IsDefault = true,
                CreatedAt = DateTime.Now,
                LastModified = DateTime.Now
            };
        }

        public AccountGroup Clone()
        {
            var clone = new AccountGroup(this.Name)
            {
                Id = this.Id,
                Icon = this.Icon,
                ColorVariant = this.ColorVariant,
                Position = this.Position,
                IsDefault = this.IsDefault,
                CreatedAt = this.CreatedAt,
                LastModified = DateTime.Now
            };

            foreach (var account in this.Accounts)
            {
                clone.Accounts.Add(account.Clone());
            }

            return clone;
        }

        public void AddAccount(Account account)
        {
            if (account != null && !Accounts.Contains(account))
            {
                account.PropertyChanged += Account_PropertyChanged;
                Accounts.Add(account);
                UpdateLastModified();
            }
        }

        public void RemoveAccount(Account account)
        {
            if (account != null && Accounts.Contains(account))
            {
                account.PropertyChanged -= Account_PropertyChanged;
                Accounts.Remove(account);
                UpdateLastModified();
            }
        }

        public bool ContainsAccount(string accountName)
        {
            return Accounts.Any(a => string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));
        }

        public Account FindAccount(string accountName)
        {
            return Accounts.FirstOrDefault(a => string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        public bool HasChanges(AccountGroup other)
        {
            if (other == null) return true;
            return Name != other.Name ||
                   Icon != other.Icon ||
                   ColorVariant != other.ColorVariant ||
                   Position != other.Position;
        }

        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Subscribe/unsubscribe to property changes for added/removed accounts
            if (e.NewItems != null)
            {
                foreach (Account account in e.NewItems)
                {
                    account.PropertyChanged += Account_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Account account in e.OldItems)
                {
                    account.PropertyChanged -= Account_PropertyChanged;
                }
            }

            UpdateLastModified();
            RefreshComputedProperties();
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Refresh favorite-related properties when an account's favorite status changes
            if (e.PropertyName == nameof(Account.IsFavorite))
            {
                OnPropertyChanged(nameof(FavoriteCount));
                OnPropertyChanged(nameof(FavoriteCountText));
                OnPropertyChanged(nameof(HasFavorites));
                OnPropertyChanged(nameof(FavoriteAccounts));
                OnPropertyChanged(nameof(GroupStatsText));
            }
        }

        private void RefreshComputedProperties()
        {
            OnPropertyChanged(nameof(AccountCount));
            OnPropertyChanged(nameof(FavoriteCount));
            OnPropertyChanged(nameof(AccountCountText));
            OnPropertyChanged(nameof(FavoriteCountText));
            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(HasFavorites));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(MostRecentAccount));
            OnPropertyChanged(nameof(LastActivityText));
            OnPropertyChanged(nameof(FavoriteAccounts));
            OnPropertyChanged(nameof(GroupStatsText));
            OnPropertyChanged(nameof(IconBackgroundColor));
            OnPropertyChanged(nameof(IconBackgroundColorWithOpacity));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                
                // Update computed properties for visual changes
                if (propertyName == nameof(ColorVariant))
                {
                    OnPropertyChanged(nameof(IconBackgroundColor));
                    OnPropertyChanged(nameof(IconBackgroundColorWithOpacity));
                }
                
                // Only update LastModified for actual data properties, not for LastModified itself
                if (propertyName != nameof(LastModified) && propertyName != nameof(CreatedAt))
                {
                    _lastModified = DateTime.Now;
                    OnPropertyChanged(nameof(LastModified));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}