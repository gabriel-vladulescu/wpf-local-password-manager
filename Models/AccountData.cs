using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountManager.Models
{
    public class AccountData
    {
        public List<AccountGroup> Groups { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";

        public int TotalGroups => Groups?.Count ?? 0;
        public int TotalAccounts => Groups?.Sum(g => g.AccountCount) ?? 0;

        public AccountData()
        {
            Groups = new List<AccountGroup>();
        }

        public AccountGroup FindGroup(string name)
        {
            return Groups?.FirstOrDefault(g => 
                string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public Account FindAccount(string accountName)
        {
            return Groups?.SelectMany(g => g.Accounts)
                         ?.FirstOrDefault(a => string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }
    }
}