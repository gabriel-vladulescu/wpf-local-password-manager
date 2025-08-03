using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AccountManager.Models.Interfaces
{
    public interface IAccountGroup : INotifyPropertyChanged
    {
        string Name { get; set; }
        ObservableCollection<Account> Accounts { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime LastModified { get; set; }
        int AccountCount { get; }

        void AddAccount(Account account);
        void RemoveAccount(Account account);
        Account FindAccount(string name);
        AccountGroup Clone();
    }
}