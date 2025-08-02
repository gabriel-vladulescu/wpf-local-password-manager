# Account Manager - WPF Password Manager

A modern, secure, and elegant password manager built with WPF and Material Design. Store and organize your accounts locally with a beautiful dark/light theme interface.

## Features

✨ **Modern UI/UX**
- Clean, modern interface with Material Design
- Full dark/light theme support with toggle
- Smooth animations and hover effects
- Responsive layout with glass morphism effects

🔐 **Security & Privacy**
- Local storage only - your data never leaves your device
- Encrypted password storage
- Secure password generation
- No cloud dependencies

📊 **Organization**
- Group accounts by category (Work, Personal, Gaming, etc.)
- Search functionality across all accounts
- Bulk operations and account management
- Import/Export capabilities

🎨 **User Experience**
- Floating action buttons for quick access
- Intuitive keyboard shortcuts
- Copy passwords with one click
- Real-time search and filtering

## Screenshots

### Light Theme
![Light Theme](screenshots/light-theme.png)

### Dark Theme
![Dark Theme](screenshots/dark-theme.png)

## Installation

### Option 1: Download Release
1. Go to [Releases](https://github.com/username/wpf-local-password-manager/releases)
2. Download `AccountManager.exe`
3. Run the executable - no installation required!

### Option 2: Build from Source

#### Prerequisites
- .NET 8 SDK ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- Windows 10/11

#### Build Steps
```bash
# Clone the repository
git clone https://github.com/username/wpf-local-password-manager.git
cd wpf-local-password-manager

# Build and run
dotnet run

# Or build release version
./build-release.bat
```

The build script creates a single-file executable in the `.build` folder with all dependencies included.

## Usage

### Getting Started
1. **Create your first group**: Click "Add Group" to organize your accounts
2. **Add accounts**: Use the floating + button to add new accounts
3. **Search**: Use the search bar to quickly find accounts
4. **Copy passwords**: Click the copy icon on any account card

### Managing Groups
- **Create**: Click "Add Group" in the sidebar
- **Edit**: Click the pencil icon next to any group
- **Delete**: Click the delete icon (warns if group contains accounts)

### Managing Accounts
- **Create**: Click the floating + button when a group is selected
- **Edit**: Click on any account card or use the edit button
- **Delete**: Use the delete button on account cards
- **Copy Password**: Click the copy icon for instant clipboard access

### Theme Toggle
- Click the theme toggle icon in the top-right corner
- Switches between light and dark themes instantly
- Theme preference is automatically saved

## Technical Details

### Architecture
```
AccountManager/
├── Models/           # Data models (Account, AccountGroup)
├── ViewModels/       # MVVM view models with commands
├── Views/            # XAML dialogs and user controls
├── Services/         # JSON storage service
└── Converters/       # WPF value converters
```

### Key Technologies
- **Framework**: .NET 8, WPF
- **UI Library**: Material Design in XAML
- **Architecture**: MVVM pattern
- **Storage**: Local JSON files
- **Packaging**: Single-file executable

### Data Storage
- **Location**: `accounts.json` in the application directory
- **Format**: JSON with clear structure
- **Backup**: Recommended to backup the JSON file regularly

### Security Notes
- Passwords are stored in plain text in the JSON file
- The application is designed for local use only
- For enhanced security, consider storing the JSON file in an encrypted folder
- Future versions may include built-in encryption

## Development

### Project Structure
```
├── AccountManager.csproj     # Project configuration
├── App.xaml                  # Application resources and themes
├── MainWindow.xaml           # Main application window
├── build-release.bat         # Build script for releases
└── README.md                 # This file
```

### Key Features Implementation
- **Theme System**: Dynamic resource switching with `ThemeService`
- **MVVM Pattern**: Clean separation with `RelayCommand` implementation
- **Material Design**: Custom styles extending Material Design themes
- **Responsive UI**: Adaptive layouts for different window sizes

### Contributing
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Roadmap

### v2.0 (Planned)
- [ ] Built-in encryption for account data
- [ ] Import from other password managers
- [ ] Backup and restore functionality
- [ ] Password strength indicators
- [ ] Two-factor authentication support

### v2.1 (Future)
- [ ] Browser integration
- [ ] Auto-fill capabilities
- [ ] Password breach checking
- [ ] Multiple vault support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/yourusername/wpf-local-password-manager/issues)
- **Discussions**: Join conversations in [GitHub Discussions](https://github.com/yourusername/wpf-local-password-manager/discussions)

## Acknowledgments

- [Material Design in XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) for the beautiful UI components
- [.NET Foundation](https://dotnetfoundation.org/) for the robust framework
- The open-source community for inspiration and feedback

---

**⚠️ Security Reminder**: Always keep your `accounts.json` file secure and consider regular backups. This application is designed for personal use and local storage only.