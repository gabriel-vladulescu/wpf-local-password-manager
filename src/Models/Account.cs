using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountManager.Models
{
    public class Account : INotifyPropertyChanged
    {
        private string _name;
        private string _username;
        private string _password;
        private string _email;
        private string _website;
        private string _notes;
        private bool _isFavorite;
        private bool _isArchived;
        private bool _isTrashed;
        private DateTime _createdDate;
        private DateTime _lastModified;
        private DateTime? _archivedDate;
        private DateTime? _trashedDate;
        private string _previousGroupId; // NEW: Track where account came from

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    UpdateLastModified();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                    UpdateLastModified();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    UpdateLastModified();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                    UpdateLastModified();
            }
        }

        public string Website
        {
            get => _website;
            set
            {
                if (SetProperty(ref _website, value))
                    UpdateLastModified();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                    UpdateLastModified();
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public bool IsArchived
        {
            get => _isArchived;
            set
            {
                if (SetProperty(ref _isArchived, value))
                {
                    if (value)
                    {
                        ArchivedDate = DateTime.Now;
                        UpdateLastModified();
                    }
                    else
                    {
                        ArchivedDate = null;
                        _previousGroupId = null; // Clear when unarchiving
                    }
                }
            }
        }

        public bool IsTrashed
        {
            get => _isTrashed;
            set
            {
                if (SetProperty(ref _isTrashed, value))
                {
                    if (value)
                    {
                        TrashedDate = DateTime.Now;
                        UpdateLastModified();
                    }
                    else
                    {
                        TrashedDate = null;
                        _previousGroupId = null; // Clear when untrashing
                    }
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

        // Alias for compatibility
        public DateTime ModifiedDate
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public DateTime? ArchivedDate
        {
            get => _archivedDate;
            set => SetProperty(ref _archivedDate, value);
        }

        public DateTime? TrashedDate
        {
            get => _trashedDate;
            set => SetProperty(ref _trashedDate, value);
        }

        // NEW: Track previous group for restore functionality
        public string PreviousGroupId
        {
            get => _previousGroupId;
            set => SetProperty(ref _previousGroupId, value);
        }

        // Computed properties for UI
        public bool HasWebsite => !string.IsNullOrWhiteSpace(Website);

        public string WebsiteDisplayUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Website))
                    return string.Empty;

                try
                {
                    var uri = new Uri(Website.StartsWith("http") ? Website : $"https://{Website}");
                    return uri.Host;
                }
                catch
                {
                    return Website;
                }
            }
        }

        public string StatusText
        {
            get
            {
                if (IsTrashed)
                    return $"Deleted on {TrashedDate:MMM dd, yyyy}";
                if (IsArchived)
                    return $"Archived on {ArchivedDate:MMM dd, yyyy}";
                return $"Modified on {LastModified:MMM dd, yyyy 'at' HH:mm}";
            }
        }

        public string StatusIcon
        {
            get
            {
                if (IsTrashed)
                    return "Delete";
                if (IsArchived)
                    return "Archive";
                return "Account";
            }
        }

        public string LastModifiedFormatted => LastModified.ToString("MMM dd, yyyy 'at' HH:mm");

        public string CreatedDateFormatted => CreatedDate.ToString("MMM dd, yyyy");

        public bool IsActive => !IsTrashed && !IsArchived;

        // NEW: UI properties for archive/trash views
        public bool CanBeFavorited => IsActive; // Only active accounts can be favorited
        public bool ShowFavoriteIcon => IsActive; // Hide favorite icon for archived/trashed

        public Account()
        {
            var now = DateTime.Now;
            _createdDate = now;
            _lastModified = now;
        }

        public Account(string name, string username, string password, string email = "", string website = "", string notes = "")
        {
            var now = DateTime.Now;
            _createdDate = now;
            _lastModified = now;
            
            _name = name;
            _username = username;
            _password = password;
            _email = email;
            _website = website;
            _notes = notes;
        }

        private void UpdateLastModified()
        {
            _lastModified = DateTime.Now;
            OnPropertyChanged(nameof(LastModified));
            OnPropertyChanged(nameof(LastModifiedFormatted));
            OnPropertyChanged(nameof(StatusText));
        }

        // Validation method
        public bool IsValid()
        {
            // An account is valid if it has a non-empty name
            // Username and Email are optional fields
            return !string.IsNullOrWhiteSpace(Name);
        }

        // Create a copy of the account
        public Account Clone()
        {
            return new Account
            {
                Name = this.Name,
                Username = this.Username,
                Password = this.Password,
                Email = this.Email,
                Website = this.Website,
                Notes = this.Notes,
                IsFavorite = this.IsFavorite,
                IsArchived = this.IsArchived,
                IsTrashed = this.IsTrashed,
                CreatedDate = this.CreatedDate,
                LastModified = this.LastModified,
                ArchivedDate = this.ArchivedDate,
                TrashedDate = this.TrashedDate,
                PreviousGroupId = this.PreviousGroupId
            };
        }

        // Update this account with values from another account
        public void UpdateFrom(Account other)
        {
            if (other == null) return;

            Name = other.Name;
            Username = other.Username;
            Password = other.Password;
            Email = other.Email;
            Website = other.Website;
            Notes = other.Notes;
            IsFavorite = other.IsFavorite;
            UpdateLastModified();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}