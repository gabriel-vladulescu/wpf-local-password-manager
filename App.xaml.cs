using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using AccountManager.Services;

namespace AccountManager
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        protected override void OnStartup(StartupEventArgs e)
        {
            // Allocate a console for this GUI application
            AllocConsole();
            
            // Redirect console output
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            
            Console.WriteLine("=== Account Manager Debug Console ===");
            Console.WriteLine("Watch for debug messages here...");
            Console.WriteLine();
            
            // Initialize theme service before showing any UI
            try
            {
                var themeService = ThemeService.Instance;
                Console.WriteLine($"Theme service initialized. Current theme: {themeService.CurrentTheme}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing theme service: {ex.Message}");
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