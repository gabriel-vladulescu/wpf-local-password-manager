using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AccountManager.Config;
using AccountManager.Core;
using AccountManager.Core.Interfaces;
using AccountManager.Models;
using AccountManager.Repositories;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Managers
{
    /// <summary>
    /// Manages application themes and appearance settings
    /// Replaces the old ThemeService with better separation of concerns
    /// </summary>
    public class ThemeManager
    {
        private readonly AppDataRepository _dataRepository;
        private ThemeSettings _currentTheme;

        public event EventHandler<string> ThemeChanged;

        public ThemeManager(AppDataRepository dataRepository)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            // Don't auto-load theme in constructor - let caller control when to initialize
            
            // Subscribe to data changes to reload theme when data is imported/changed
            _dataRepository.DataChanged += OnDataChanged;
        }

        /// <summary>
        /// Initialize the theme manager and load theme settings
        /// Should be called early in application startup
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadThemeAsync();
        }

        public string CurrentTheme => _currentTheme?.CurrentTheme ?? "Light";

        public async Task LoadThemeAsync()
        {
            try
            {
                var data = await _dataRepository.GetAsync();
                _currentTheme = data?.Theme ?? new ThemeSettings();
                
                // Apply the loaded theme to the application
                await ApplyThemeAsync(CurrentTheme);
            }
            catch (Exception ex)
            {
                // Show error notification for theme loading issues
                try
                {
                    var notificationService = ServiceContainer.Instance.NotificationService;
                    notificationService?.ShowError($"Error loading theme: {ex.Message}", "Theme Error");
                }
                catch { } // Prevent recursive errors
                _currentTheme = new ThemeSettings();
                await ApplyThemeAsync("Light"); // Default to Light theme
            }
        }

        public async Task<bool> SetThemeAsync(string themeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(themeName) || CurrentTheme == themeName)
                    return false;

                var data = await _dataRepository.GetAsync();
                if (data != null)
                {
                    data.Theme ??= new ThemeSettings();
                    data.Theme.CurrentTheme = themeName;
                    _currentTheme = data.Theme;

                    var success = await _dataRepository.SaveAsync(data);
                    if (success)
                    {
                        await ApplyThemeAsync(themeName);
                        ThemeChanged?.Invoke(this, themeName);
                    }
                    return success;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task ToggleThemeAsync()
        {
            var newTheme = CurrentTheme == "Light" ? "Dark" : "Light";
            await SetThemeAsync(newTheme);
        }

        public bool IsLightTheme => CurrentTheme == "Light";
        public bool IsDarkTheme => CurrentTheme == "Dark";
        public bool IsDarkMode => IsDarkTheme;
        public string ThemeDisplayName => IsLightTheme ? "Dark" : "Light";

        private async Task ApplyThemeAsync(string themeName)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var paletteHelper = new PaletteHelper();
                    var theme = paletteHelper.GetTheme();
                    var isDark = themeName == "Dark";

                    // Set the base theme
                    theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light);

                    // Apply custom colors using the old ThemeService approach
                    if (isDark)
                    {
                        ApplyDarkTheme(theme);
                    }
                    else
                    {
                        ApplyLightTheme(theme);
                    }

                    // Apply the theme
                    paletteHelper.SetTheme(theme);

                    // Update application resources for custom styles
                    UpdateCustomResources(isDark);
                }
                catch (Exception ex)
                {
                }
            });
        }

        private void ApplyDarkTheme(ITheme theme)
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

        private void ApplyLightTheme(ITheme theme)
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

        private void UpdateCustomResources(bool isDark)
        {
            var app = Application.Current;
            if (app?.Resources == null) return;

            // Update custom application resources based on theme
            if (isDark)
            {
                // Dark theme resources
                app.Resources["SidebarColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SidebarBackground);
                app.Resources["AppBarBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.AppBarBackgroundColor);

                app.Resources["MainBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Background);
                app.Resources["CardBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.CardBackground);
                app.Resources["CardBorderColor"] = ColorConstants.ToBrush(ColorConstants.Dark.Border);
                app.Resources["CardBorderHoverColor"] = ColorConstants.ToBrush(ColorConstants.Dark.CardBorderHover);
                app.Resources["TextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.TextPrimary);
                app.Resources["TextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Dark.TextSecondary);
                app.Resources["TextAccentColor"] = ColorConstants.ToBrush(ColorConstants.Dark.TextAccent);
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
                app.Resources["InputBorderFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputBorderFocus);
                app.Resources["InputPlaceholderBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputPlaceholder);
                app.Resources["InputTextBrush"] = ColorConstants.ToBrush(ColorConstants.Dark.InputText);
                app.Resources["InputFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
                
                // UI State colors for dark theme
                app.Resources["HoverBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.HoverBackground);
                app.Resources["SelectedBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.SelectedBackground);
                app.Resources["GlowColor"] = ColorConstants.ToBrush(ColorConstants.Dark.GlowColor);
                app.Resources["ContextMenuBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Dark.ContextMenuBackground);
                app.Resources["ContextMenuBorderColor"] = ColorConstants.ToBrush(ColorConstants.Dark.ContextMenuBorder);
            }
            else
            {
                // Light theme resources
                app.Resources["SidebarColor"] = ColorConstants.ToBrush(ColorConstants.Light.SidebarBackground);
                app.Resources["AppBarBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.AppBarBackgroundColor);

                app.Resources["MainBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.Background);
                app.Resources["CardBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.CardBackground);
                app.Resources["CardBorderColor"] = ColorConstants.ToBrush(ColorConstants.Light.Border);
                app.Resources["CardBorderHoverColor"] = ColorConstants.ToBrush(ColorConstants.Light.CardBorderHover);
                app.Resources["TextPrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.TextPrimary);
                app.Resources["TextSecondaryColor"] = ColorConstants.ToBrush(ColorConstants.Light.TextSecondary);
                app.Resources["TextAccentColor"] = ColorConstants.ToBrush(ColorConstants.Light.TextAccent);
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
                app.Resources["InputBorderFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputBorderFocus);
                app.Resources["InputPlaceholderBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputPlaceholder);
                app.Resources["InputTextBrush"] = ColorConstants.ToBrush(ColorConstants.Light.InputText);
                app.Resources["InputFocusBrush"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
                
                // UI State colors for light theme
                app.Resources["HoverBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.HoverBackground);
                app.Resources["SelectedBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.SelectedBackground);
                app.Resources["GlowColor"] = ColorConstants.ToBrush(ColorConstants.Light.GlowColor);
                app.Resources["ContextMenuBackgroundColor"] = ColorConstants.ToBrush(ColorConstants.Light.ContextMenuBackground);
                app.Resources["ContextMenuBorderColor"] = ColorConstants.ToBrush(ColorConstants.Light.ContextMenuBorder);
            }

            // Common colors that don't change
            app.Resources["PrimaryColor"] = ColorConstants.ToBrush(ColorConstants.Common.Primary);
            app.Resources["AccentColor"] = ColorConstants.ToBrush(ColorConstants.Common.Accent);
            app.Resources["SuccessColor"] = ColorConstants.ToBrush(ColorConstants.Common.Success);
            app.Resources["DangerColor"] = ColorConstants.ToBrush(ColorConstants.Common.Danger);
            app.Resources["WarningColor"] = ColorConstants.ToBrush(ColorConstants.Common.Warning);
        }

        /// <summary>
        /// Handle data changes from repository (e.g., import operations)
        /// </summary>
        private async void OnDataChanged(object sender, AppData newData)
        {
            try
            {
                // Reload theme when data changes (e.g., after import) - ensure we're on the UI thread
                await Application.Current?.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadThemeAsync();
                });
            }
            catch (Exception ex)
            {
                // Log error but don't fail the theme system
                try
                {
                    var notificationService = ServiceContainer.Instance.NotificationService;
                    notificationService?.ShowError($"Error reloading theme after data change: {ex.Message}", "Theme Reload Error");
                }
                catch { }
            }
        }
    }
}