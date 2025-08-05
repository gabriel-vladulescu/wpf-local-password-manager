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
        private string _name = "";
        private ObservableCollection<Account> _accounts = new();
        private DateTime _createdAt = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;

        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set 
            { 
                if (_accounts != null)
                    _accounts.CollectionChanged -= Accounts_CollectionChanged;
                    
                SetProperty(ref _accounts, value);
                
                if (_accounts != null)
                    _accounts.CollectionChanged += Accounts_CollectionChanged;
                    
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
        public string AccountCountText => AccountCount == 1 ? "1 account" : $"{AccountCount} accounts";

        [JsonIgnore]
        public bool HasAccounts => AccountCount > 0;

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

        public AccountGroup()
        {
            var now = DateTime.Now;
            _createdAt = now;
            _lastModified = now;
            _accounts = new ObservableCollection<Account>();
            _accounts.CollectionChanged += Accounts_CollectionChanged;
        }

        public AccountGroup(string name) : this()
        {
            Name = name;
        }

        public AccountGroup Clone()
        {
            var clone = new AccountGroup(this.Name)
            {
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
                Accounts.Add(account);
                UpdateLastModified();
            }
        }

        public void RemoveAccount(Account account)
        {
            if (account != null && Accounts.Contains(account))
            {
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
            return Name != other.Name;
        }

        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLastModified();
            RefreshComputedProperties();
        }

        private void RefreshComputedProperties()
        {
            OnPropertyChanged(nameof(AccountCount));
            OnPropertyChanged(nameof(AccountCountText));
            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(MostRecentAccount));
            OnPropertyChanged(nameof(LastActivityText));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                
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