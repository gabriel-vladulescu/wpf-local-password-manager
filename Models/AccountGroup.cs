using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using AccountManager.Models.Interfaces;

namespace AccountManager.Models
{
    public class AccountGroup : IAccountGroup, INotifyPropertyChanged
    {
        private string _name = "";
        private ObservableCollection<Account> _accounts = new();
        private DateTime _createdDate = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;

        public string Name
        {
            get => _name;
            set 
            { 
                if (SetProperty(ref _name, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value ?? new ObservableCollection<Account>());
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public int AccountCount => Accounts?.Count ?? 0;

        public AccountGroup()
        {
            Accounts = new ObservableCollection<Account>();
            // Subscribe to collection changes to update LastModified
            Accounts.CollectionChanged += (s, e) => LastModified = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void AddAccount(Account account)
        {
            if (account != null && !Accounts.Contains(account))
            {
                Accounts.Add(account);
            }
        }

        public void RemoveAccount(Account account)
        {
            if (account != null)
            {
                Accounts.Remove(account);
            }
        }

        public Account FindAccount(string name)
        {
            return Accounts?.FirstOrDefault(a => 
                string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public AccountGroup Clone()
        {
            var cloned = new AccountGroup
            {
                Name = Name,
                CreatedDate = CreatedDate,
                LastModified = LastModified
            };

            foreach (var account in Accounts)
            {
                cloned.Accounts.Add(account.Clone());
            }

            return cloned;
        }
    }
}