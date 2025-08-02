using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using MaterialDesignThemes.Wpf;

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
            theme.SetPrimaryColor(System.Windows.Media.Color.FromRgb(99, 102, 241)); // #6366F1
            theme.SetSecondaryColor(System.Windows.Media.Color.FromRgb(139, 92, 246)); // #8B5CF6
            
            // Dark theme specific colors
            theme.Paper = System.Windows.Media.Color.FromRgb(15, 23, 42);        // #0F172A
            theme.CardBackground = System.Windows.Media.Color.FromRgb(30, 41, 59); // #1E293B
            theme.ToolBarBackground = System.Windows.Media.Color.FromRgb(30, 41, 59);
            theme.Body = System.Windows.Media.Color.FromRgb(241, 245, 249);       // #F1F5F9
            theme.BodyLight = System.Windows.Media.Color.FromRgb(148, 163, 184);  // #94A3B8
            theme.ColumnHeader = System.Windows.Media.Color.FromRgb(71, 85, 105); // #475569
            theme.CheckBoxOff = System.Windows.Media.Color.FromRgb(100, 116, 139); // #64748B
            theme.Divider = System.Windows.Media.Color.FromRgb(51, 65, 85);       // #334155
            theme.Selection = System.Windows.Media.Color.FromRgb(99, 102, 241);   // #6366F1 with opacity
        }

        private void ApplyLightTheme(Theme theme)
        {
            // Primary colors
            theme.SetPrimaryColor(System.Windows.Media.Color.FromRgb(99, 102, 241)); // #6366F1
            theme.SetSecondaryColor(System.Windows.Media.Color.FromRgb(139, 92, 246)); // #8B5CF6
            
            // Light theme specific colors
            theme.Paper = System.Windows.Media.Color.FromRgb(255, 255, 255);      // #FFFFFF
            theme.CardBackground = System.Windows.Media.Color.FromRgb(255, 255, 255);
            theme.ToolBarBackground = System.Windows.Media.Color.FromRgb(248, 250, 252);
            theme.Body = System.Windows.Media.Color.FromRgb(30, 41, 59);          // #1E293B
            theme.BodyLight = System.Windows.Media.Color.FromRgb(100, 116, 139);  // #64748B
            theme.ColumnHeader = System.Windows.Media.Color.FromRgb(148, 163, 184); // #94A3B8
            theme.CheckBoxOff = System.Windows.Media.Color.FromRgb(203, 213, 225); // #CBD5E1
            theme.Divider = System.Windows.Media.Color.FromRgb(226, 232, 240);    // #E2E8F0
            theme.Selection = System.Windows.Media.Color.FromRgb(99, 102, 241);   // #6366F1
        }

        private void UpdateApplicationResources()
        {
            var app = Application.Current;
            if (app?.Resources == null) return;

            // Update custom application resources based on theme
            if (CurrentTheme == AppTheme.Dark)
            {
                // Dark theme resources
                app.Resources["SidebarColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 41, 59)); // #1E293B - Dark sidebar
                app.Resources["MainBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(15, 23, 42)); // #0F172A - Dark background
                app.Resources["CardBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 41, 59)); // #1E293B
                app.Resources["CardBorderColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 65, 85)); // #334155
                app.Resources["CardBorderHoverColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(99, 102, 241)); // #6366F1
                app.Resources["TextPrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(241, 245, 249)); // #F1F5F9
                app.Resources["TextSecondaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(148, 163, 184)); // #94A3B8
                app.Resources["BorderColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 65, 85)); // #334155
                app.Resources["InputBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 65, 85)); // #334155
                app.Resources["SearchBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 65, 85)); // #334155
                app.Resources["EmptyStateBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(51, 65, 85)); // #334155
                
                // Sidebar text colors for dark theme (WHITE TEXT)
                app.Resources["SidebarTextPrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 255, 255)); // White
                app.Resources["SidebarTextSecondaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(148, 163, 184)); // #94A3B8
            }
            else
            {
                // Light theme resources
                app.Resources["SidebarColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 255, 255)); // #FFFFFF - White sidebar
                app.Resources["MainBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(241, 245, 249)); // #F1F5F9 - Light gray background
                app.Resources["CardBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 255, 255)); // #FFFFFF
                app.Resources["CardBorderColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(226, 232, 240)); // #E2E8F0
                app.Resources["CardBorderHoverColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(99, 102, 241)); // #6366F1
                app.Resources["TextPrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 41, 59)); // #1E293B
                app.Resources["TextSecondaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(100, 116, 139)); // #64748B
                app.Resources["BorderColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(226, 232, 240)); // #E2E8F0
                app.Resources["InputBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(248, 250, 252)); // #F8FAFC
                app.Resources["SearchBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(248, 250, 252)); // #F8FAFC
                app.Resources["EmptyStateBackgroundColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(248, 250, 252)); // #F8FAFC
                
                // Sidebar text colors for light theme (DARK TEXT)
                app.Resources["SidebarTextPrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(30, 41, 59)); // #1E293B - Dark text
                app.Resources["SidebarTextSecondaryColor"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(100, 116, 139)); // #64748B - Gray text
            }

            // Force UI refresh
            Console.WriteLine($"Theme switched to: {CurrentTheme}");
            Console.WriteLine($"Sidebar color set to: {(CurrentTheme == AppTheme.Dark ? "#1E293B (dark)" : "#FFFFFF (white)")}");
            
            // Common colors that don't change
            app.Resources["PrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(99, 102, 241)); // #6366F1
            app.Resources["AccentColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(139, 92, 246)); // #8B5CF6
            app.Resources["SuccessColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(16, 185, 129)); // #10B981
            app.Resources["DangerColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(239, 68, 68)); // #EF4444
            app.Resources["WarningColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(245, 158, 11)); // #F59E0B
        }

        private void LoadThemeFromSettings()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    var json = File.ReadAllText(SettingsFileName);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null && Enum.IsDefined(typeof(AppTheme), settings.Theme))
                    {
                        CurrentTheme = settings.Theme;
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
                var settings = new AppSettings { Theme = CurrentTheme };
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

    internal class AppSettings
    {
        public AppTheme Theme { get; set; } = AppTheme.Light;
    }
}