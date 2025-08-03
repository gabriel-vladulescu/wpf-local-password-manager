using System;
using System.ComponentModel;

namespace AccountManager.Models.Interfaces
{
    public interface IAccount : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string Website { get; set; }
        string Notes { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime LastModified { get; set; }

        Account Clone();
    }
}