using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AccountManager.Models
{
    public class Account : INotifyPropertyChanged
    {
        private string _name = "";
        private string _username = "";
        private string _email = "";
        private string _password = "";
        private string _website = "";
        private string _notes = "";
        private DateTime _createdAt = DateTime.Now;
        private DateTime _lastModified = DateTime.Now;

        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        public string Username
        {
            get => _username;
            set { SetProperty(ref _username, value); }
        }

        public string Email
        {
            get => _email;
            set { SetProperty(ref _email, value); }
        }

        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); }
        }

        public string Website
        {
            get => _website;
            set { SetProperty(ref _website, value); }
        }

        public string Notes
        {
            get => _notes;
            set { SetProperty(ref _notes, value); }
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
        public string DisplayUsername => string.IsNullOrWhiteSpace(Username) ? Email : Username;

        [JsonIgnore]
        public string WebsiteDisplayUrl => FormatWebsiteUrl(Website);

        [JsonIgnore]
        public bool HasWebsite => !string.IsNullOrWhiteSpace(Website);

        [JsonIgnore]
        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        [JsonIgnore]
        public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");

        [JsonIgnore]
        public string LastModifiedFormatted => LastModified.ToString("MMM dd, yyyy 'at' HH:mm");

        public Account()
        {
            var now = DateTime.Now;
            _createdAt = now;
            _lastModified = now;
        }

        public Account Clone()
        {
            return new Account
            {
                Name = this.Name,
                Username = this.Username,
                Email = this.Email,
                Password = this.Password,
                Website = this.Website,
                Notes = this.Notes,
                CreatedAt = this.CreatedAt,
                LastModified = DateTime.Now
            };
        }

        public void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Password);
        }

        public bool HasChanges(Account other)
        {
            if (other == null) return true;
            
            return Name != other.Name ||
                   Username != other.Username ||
                   Email != other.Email ||
                   Password != other.Password ||
                   Website != other.Website ||
                   Notes != other.Notes;
        }

        private static string FormatWebsiteUrl(string website)
        {
            if (string.IsNullOrWhiteSpace(website))
                return "";

            var trimmed = website.Trim();
            
            // Remove protocol for display
            if (trimmed.StartsWith("https://"))
                return trimmed.Substring(8);
            if (trimmed.StartsWith("http://"))
                return trimmed.Substring(7);
                
            return trimmed;
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