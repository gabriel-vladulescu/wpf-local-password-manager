using System;
using System.Windows;
using System.Reflection;
using AccountManager.Services;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SetupGlobalInfo();
                SetupGlobalServices();
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

        private void SetupGlobalServices()
        {
            _ = ThemeService.Instance;
        }

        // protected override void OnExit(ExitEventArgs e)
        // {
        //     // Do actions on exit
        //     base.OnExit(e);
        // }
    }
}