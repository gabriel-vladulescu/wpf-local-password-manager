using System.ComponentModel;

namespace AccountManager.Services.Interfaces
{
    public enum AppTheme
    {
        Light,
        Dark,
        Auto
    }

    public interface IThemeService : INotifyPropertyChanged
    {
        AppTheme CurrentTheme { get; }
        bool IsDarkMode { get; }
        string ThemeDisplayName { get; }

        void ToggleTheme();
        void SetTheme(AppTheme theme);
        void ApplySystemTheme();
    }
}