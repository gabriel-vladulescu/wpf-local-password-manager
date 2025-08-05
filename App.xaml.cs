using System;
using System.Windows;
using AccountManager.Services;

namespace AccountManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Initialize theme service before showing any UI
            try
            {
                var themeService = ThemeService.Instance;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing theme service: {ex.Message}", 
                              "Initialization Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Theme settings are automatically saved by the ThemeService
            base.OnExit(e);
        }
    }
}