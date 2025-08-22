using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using MaterialDesignThemes.Wpf;
using AccountManager.Config;

namespace AccountManager.Services
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public class ThemeService : INotifyPropertyChanged
    {
        private static ThemeService _instance;
        private const string SettingsFileName = "settings.json";
        private AppTheme _currentTheme = AppTheme.Light;
        private readonly PaletteHelper _paletteHelper;

        public static ThemeService Instance => _instance ??= new ThemeService();

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDarkMode));
                    OnPropertyChanged(nameof(ThemeDisplayName));
                }
            }
        }

        public bool IsDarkMode => CurrentTheme == AppTheme.Dark;
        public string ThemeDisplayName => CurrentTheme == AppTheme.Dark ? "Dark" : "Light";

        private ThemeService()
        {
            _paletteHelper = new PaletteHelper();
            LoadThemeFromSettings();
        }

        public void ToggleTheme()
        {
            var newTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
            SetTheme(newTheme);
        }

        public void SetTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            ApplyTheme();
            SaveThemeToSettings();
        }

        private void ApplyTheme()
        {
            var theme = (Theme)_paletteHelper.GetTheme();

            // Set the base theme
            theme.SetBaseTheme(CurrentTheme == AppTheme.Dark
                ? MaterialDesignThemes.Wpf.Theme.Dark
                : MaterialDesignThemes.Wpf.Theme.Light);

            // Apply custom colors
            if (CurrentTheme == AppTheme.Dark)
            {
                ApplyDarkTheme(theme);
            }
            else
            {
                ApplyLightTheme(theme);
            }
            
            _paletteHelper.SetTheme(theme);
            
            // Update application resources for custom styles
            UpdateApplicationResources();
        }

        private void ApplyDarkTheme(Theme theme)
        {
            // Primary colors
            theme.SetPrimaryColor(ColorConstants.FromHex(ColorConstants.Common.Primary));
            theme.SetSecondaryColor(ColorConstants.FromHex(ColorConstants.Common.Accent));
            
            // Dark theme specific colors
            theme.Paper = ColorConstants.FromHex(ColorConstants.Dark.Background);
            theme.CardBackground = ColorConstants.FromHex(ColorConstants.Dark.Surface);
            theme.ToolBarBackground = ColorConstants.FromHex(ColorConstants.Dark.Surface);
            theme.Body = ColorConstants.FromHex(ColorConstants.Dark.TextPrimary);
            theme.BodyLight = ColorConstants.FromHex(ColorConstants.Dark.TextSecondary);
            theme.ColumnHeader = ColorConstants.FromHex(ColorConstants.Dark.InputBorder);
            theme.CheckBoxOff = ColorConstants.FromHex(ColorConstants.Dark.TextSecondary);
            theme.Divider = ColorConstants.FromHex(ColorConstants.Dark.Border);
            theme.Selection = ColorConstants.FromHex(ColorConstants.Common.Primary);
        }

        private void ApplyLightTheme(Theme theme)
        {
            // Primary colors
            theme.SetPrimaryColor(ColorConstants.FromHex(ColorConstants.Common.Primary));
            theme.SetSecondaryColor(ColorConstants.FromHex(ColorConstants.Common.Accent));
            
            // Light theme specific colors
            theme.Paper = ColorConstants.FromHex(ColorConstants.Light.Surface);
            theme.CardBackground = ColorConstants.FromHex(ColorConstants.Light.Surface);
            theme.ToolBarBackground = ColorConstants.FromHex(ColorConstants.Light.Background);
            theme.Body = ColorConstants.FromHex(ColorConstants.Light.TextPrimary);
            theme.BodyLight = ColorConstants.FromHex(ColorConstants.Light.TextSecondary);
            theme.ColumnHeader = ColorConstants.FromHex(ColorConstants.Light.TextSecondary);
            theme.CheckBoxOff = ColorConstants.FromHex(ColorConstants.Light.Border);
            theme.Divider = ColorConstants.FromHex(ColorConstants.Light.Border);
            theme.Selection = ColorConstants.FromHex(ColorConstants.Common.Primary);
        }

        private void UpdateApplicationResources()
        {
            var app = Application.Current;
            if (app?.Resources == null) return;

            // Update custom application resources based on theme
            if (CurrentTheme == AppTheme.Dark)
            {
                // Dark theme resources
                app.Resources["SidebarColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SidebarBackground);
                app.Resources["AppBarBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.AppBarBackgroundColor);

                app.Resources["MainBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Background);
                app.Resources["CardBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Surface);
                app.Resources["CardBorderColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Border);
                app.Resources["CardBorderHoverColor"] = ColorConstants.ToBrush(ColorConstants.Dark.CardBorderHover);
                app.Resources["TextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.TextPrimary);
                app.Resources["TextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.TextSecondary);
                app.Resources["BorderColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Border);
                app.Resources["InputBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.InputBackground);
                app.Resources["InputLabelBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.InputLabelBackground);
                app.Resources["InputLabelTextColor"] = ColorConstants.ToBrush(ColorConstants.Dark.InputLabelText);
                app.Resources["SearchBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SearchBackground);
                app.Resources["EmptyStateBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.EmptyStateBackground);
                
                // Sidebar text colors for dark theme
                app.Resources["SidebarTextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SidebarTextPrimary);
                app.Resources["SidebarTextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SidebarTextSecondary);

                // Custom input brushes for dark theme
                app.Resources["InputBackgroundBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputBackground);
                app.Resources["InputBorderBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputBorder);
                app.Resources["InputTextBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputText);
                app.Resources["InputFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
            }
            else
            {
                // Light theme resources
                app.Resources["SidebarColor"] = ColorConstants.ToBrush(ColorConstants.Light.SidebarBackground);
                app.Resources["AppBarBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.AppBarBackgroundColor);


                app.Resources["MainBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.Background);
                app.Resources["CardBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.Surface);
                app.Resources["CardBorderColor"] = ColorConstants.ToBrush(ColorConstants.Light.Border);
                app.Resources["CardBorderHoverColor"] = ColorConstants.ToBrush(ColorConstants.Light.CardBorderHover);
                app.Resources["TextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.TextPrimary);
                app.Resources["TextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.TextSecondary);
                app.Resources["BorderColor"] = ColorConstants.ToBrush(ColorConstants.Light.Border);
                app.Resources["InputBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.InputBackground);
                app.Resources["InputLabelBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.InputLabelBackground);
                app.Resources["InputLabelTextColor"] = ColorConstants.ToBrush(ColorConstants.Light.InputLabelText);
                app.Resources["SearchBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.SearchBackground);
                app.Resources["EmptyStateBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.EmptyStateBackground);
                
                // Sidebar text colors for light theme
                app.Resources["SidebarTextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.SidebarTextPrimary);
                app.Resources["SidebarTextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.SidebarTextSecondary);

                // Custom input brushes for light theme
                app.Resources["InputBackgroundBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputBackground);
                app.Resources["InputBorderBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputBorder);
                app.Resources["InputTextBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputText);
                app.Resources["InputFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
            }

            // Common colors that don't change
            app.Resources["PrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
            app.Resources["AccentColor"] = ColorConstants.ToBrush(ColorConstants.Common.Accent);
            app.Resources["SuccessColor"] = ColorConstants.ToBrush(ColorConstants.Common.Success);
            app.Resources["DangerColor"] = ColorConstants.ToBrush(ColorConstants.Common.Danger);
            app.Resources["WarningColor"] = ColorConstants.ToBrush(ColorConstants.Common.Warning);
        }

        private void LoadThemeFromSettings()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    var json = File.ReadAllText(SettingsFileName);
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    if (settings != null && Enum.TryParse<AppTheme>(settings.Theme, out var theme))
                    {
                        CurrentTheme = theme;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme settings: {ex.Message}");
                // Default to light theme if loading fails
                CurrentTheme = AppTheme.Light;
            }
            
            // Apply the loaded theme
            ApplyTheme();
        }

        private void SaveThemeToSettings()
        {
            try
            {
                var settings = new ThemeSettings { Theme = CurrentTheme.ToString() };
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme settings: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Separate settings class for theme-specific settings
    internal class ThemeSettings
    {
        public string Theme { get; set; } = "Light";
    }
}