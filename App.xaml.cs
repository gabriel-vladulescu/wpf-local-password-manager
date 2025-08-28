using System;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using AccountManager.Core;

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
                SetupGlobalInfo();
                await SetupGlobalServicesAsync();
                
                // Create and show MainWindow only after theme is fully loaded
                CreateAndShowMainWindow();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load initial configuration", ex);
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
            
            // Initialize custom path from configuration
            serviceContainer.InitializeCustomPath();
            
            // Initialize theme manager early and await theme loading before UI shows
            var themeManager = serviceContainer.ThemeManager;
            await themeManager.InitializeAsync();
        }

        private void CreateAndShowMainWindow()
        {
            // Create MainWindow only after theme is fully initialized
            var mainWindow = new Views.Window.MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

    }
}