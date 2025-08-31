using System;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using AccountManager.Core;
using AccountManager.UI;

namespace AccountManager
{
    public partial class App : Application
    {
        public static string AppTitle { get; private set; }
        public static string AppVersion { get; private set; }
        public static string AppProduct { get; private set; }
        public static string AppCompany { get; private set; }
        public static string AppDescription { get; private set; }
        public static string AppCopyright { get; private set; }
        public static string AppTitleWithVersion { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("App startup: SetupGlobalInfo");
                SetupGlobalInfo();
                
                System.Diagnostics.Debug.WriteLine("App startup: SetupGlobalServicesAsync");
                await SetupGlobalServicesAsync();
                
                System.Diagnostics.Debug.WriteLine("App startup: CreateMainWindow");
                // Create MainWindow first (but don't show yet) so DialogManager can initialize
                CreateMainWindow();
                
                System.Diagnostics.Debug.WriteLine("App startup: InitializeThemeAsync");
                // Initialize theme manager first
                await InitializeThemeAsync();
                
                System.Diagnostics.Debug.WriteLine("App startup: ShowMainWindow");
                // Show MainWindow first so the visual tree is ready for dialogs
                ShowMainWindow();
                
                // Give the UI a moment to render before showing dialogs
                await Task.Delay(100);
                
                System.Diagnostics.Debug.WriteLine("App startup: InitializeEncryptionAsync");
                // Initialize encryption and prompt for passphrase if needed
                var encryptionReady = await InitializeEncryptionAsync();
                if (!encryptionReady)
                {
                    System.Diagnostics.Debug.WriteLine("App startup: Encryption cancelled, shutting down");
                    // User cancelled passphrase entry or encryption failed
                    Shutdown();
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("App startup: Reload data after encryption");
                // Clear cached data and reload everything now that encryption is available
                var serviceContainer = ServiceContainer.Instance;
                var dataRepository = serviceContainer.DataRepository;
                var themeManager = serviceContainer.ThemeManager;
                var configurationManager = serviceContainer.ConfigurationManager;
                
                // Clear the cached data so it gets reloaded with the correct passphrase
                dataRepository.InvalidateCache();
                
                await themeManager.ReloadThemeAfterEncryptionAsync();
                await configurationManager.ReloadSettingsAfterEncryptionAsync();
                
                System.Diagnostics.Debug.WriteLine("App startup: InitializeDataAsync");
                // Now trigger data loading after encryption is fully ready
                await InitializeDataAsync();
                
                System.Diagnostics.Debug.WriteLine("App startup: Complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App startup error: {ex}");
                MessageBox.Show($"Critical startup error: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private void SetupGlobalInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // Read from .csproj properties
            AppTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown App";
            AppProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? AppTitle;
            AppCompany = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";
            AppDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            AppCopyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
            
            // Version handling
            var version = assembly.GetName().Version;
            if (version != null) AppVersion = $"v{version.Major}.{version.Minor}.{version.Build}";
            AppTitleWithVersion = $"{AppTitle} {AppVersion}";
            
            // Add to resources for binding
            Resources["AppTitle"] = AppTitle;
            Resources["AppVersion"] = AppVersion;
            Resources["AppProduct"] = AppProduct;
            Resources["AppCompany"] = AppCompany;
            Resources["AppDescription"] = AppDescription;
            Resources["AppCopyright"] = AppCopyright;
            Resources["AppTitleWithVersion"] = AppTitleWithVersion;
        }

        private async Task SetupGlobalServicesAsync()
        {
            // Initialize the new service container
            var serviceContainer = ServiceContainer.Instance;
            
            // Complete async initialization including path loading
            await serviceContainer.CompleteInitializationAsync();
            
            // Note: ThemeManager initialization moved to after encryption is ready
        }

        private async Task<bool> InitializeEncryptionAsync()
        {
            try
            {
                var serviceContainer = ServiceContainer.Instance;
                var startupEncryptionService = serviceContainer.StartupEncryptionService;
                
                return await startupEncryptionService.InitializeEncryptionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing encryption: {ex.Message}", "Encryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task InitializeThemeAsync()
        {
            try
            {
                var serviceContainer = ServiceContainer.Instance;
                var themeManager = serviceContainer.ThemeManager;
                await themeManager.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                // Initialize configuration manager settings after encryption is ready
                var serviceContainer = ServiceContainer.Instance;
                var configurationManager = serviceContainer.ConfigurationManager;
                configurationManager.LoadSettings();
                
                // Get the MainViewModel and trigger data loading
                if (MainWindow?.DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    mainViewModel.LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading application data: {ex.Message}", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateMainWindow()
        {
            // Create MainWindow (but don't show yet)
            var mainWindow = new Views.Window.MainWindow();
            MainWindow = mainWindow;
            
            // Initialize DialogManager now that MainWindow exists
            var dialogManager = ServiceContainer.Instance.DialogManager as DialogManager;
            dialogManager?.Initialize(mainWindow);
        }

        private void ShowMainWindow()
        {
            // Show the MainWindow after everything is ready
            MainWindow?.Show();
        }

    }
}