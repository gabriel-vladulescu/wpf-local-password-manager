using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AccountManager.Models.Interfaces;

namespace AccountManager.Models
{
    public class Account : IAccount, INotifyPropertyChanged
    {
        private string _name = "";
        private string _username = "";
        private string _email = "";
        private string _password = "";
        private string _website = "";
        private string _notes = "";
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

        public string Username
        {
            get => _username;
            set 
            { 
                if (SetProperty(ref _username, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Email
        {
            get => _email;
            set 
            { 
                if (SetProperty(ref _email, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Password
        {
            get => _password;
            set 
            { 
                if (SetProperty(ref _password, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Website
        {
            get => _website;
            set 
            { 
                if (SetProperty(ref _website, value))
                {
                    LastModified = DateTime.Now;
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set 
            { 
                if (SetProperty(ref _notes, value))
                {
                    LastModified = DateTime.Now;
                }
            }
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

        public Account Clone()
        {
            return new Account
            {
                Name = Name,
                Username = Username,
                Email = Email,
                Password = Password,
                Website = Website,
                Notes = Notes,
                CreatedDate = CreatedDate,
                LastModified = LastModified
            };
        }
    }
}