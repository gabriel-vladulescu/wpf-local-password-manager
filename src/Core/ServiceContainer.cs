using System;
using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Storage;
using AccountManager.Infrastructure.Serialization;
using AccountManager.Infrastructure.Configuration;
using AccountManager.Repositories;
using AccountManager.Managers;
using AccountManager.UI;

namespace AccountManager.Core
{
    /// <summary>
    /// Simple service container for dependency injection
    /// Replaces the old singleton pattern with proper DI
    /// </summary>
    public class ServiceContainer
    {
        private static ServiceContainer _instance;
        
        // Core Infrastructure
        public IFileStorage FileStorage { get; private set; }
        public ISerializer Serializer { get; private set; }
        public IPathProvider PathProvider { get; private set; }
        public IDialogManager DialogManager { get; private set; }
        public INotificationService NotificationService { get; private set; }
        
        // Repositories
        public AppDataRepository DataRepository { get; private set; }
        
        // Managers
        public ApplicationStateManager StateManager { get; private set; }
        public ThemeManager ThemeManager { get; private set; }
        public ImportExportManager ImportExportManager { get; private set; }
        public IConfigurationManager ConfigurationManager { get; private set; }

        public static ServiceContainer Instance => _instance ??= new ServiceContainer();

        private ServiceContainer()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                // Initialize core infrastructure (no dependencies)
                FileStorage = new FileStorage();
                NotificationService = UI.NotificationService.Instance;
                DialogManager = UI.DialogManager.Instance;
                
                // Initialize infrastructure with dependencies
                PathProvider = new PathProvider(FileStorage);
                Serializer = new JsonSerializer(FileStorage);
                
                // Initialize repositories
                DataRepository = new AppDataRepository(Serializer, PathProvider);
                
                // Initialize managers
                ApplicationStateManager.Initialize(DataRepository);
                StateManager = ApplicationStateManager.Instance;
                ThemeManager = new ThemeManager(DataRepository);
                ImportExportManager = new ImportExportManager(DataRepository, DialogManager, NotificationService);
                ConfigurationManager = new ConfigurationManager(DataRepository);

                System.Diagnostics.Debug.WriteLine("ServiceContainer initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing ServiceContainer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initializes the path provider with custom path from settings
        /// Called after configuration is loaded
        /// </summary>
        public void InitializeCustomPath()
        {
            try
            {
                var customPath = ConfigurationManager.CustomDataPath;
                if (!string.IsNullOrEmpty(customPath))
                {
                    PathProvider.SetCustomDataPath(customPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing custom path: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a service instance by type
        /// </summary>
        public T GetService<T>() where T : class
        {
            var type = typeof(T);
            
            if (type == typeof(IFileStorage)) return FileStorage as T;
            if (type == typeof(ISerializer)) return Serializer as T;
            if (type == typeof(IPathProvider)) return PathProvider as T;
            if (type == typeof(IDialogManager)) return DialogManager as T;
            if (type == typeof(INotificationService)) return NotificationService as T;
            if (type == typeof(AppDataRepository)) return DataRepository as T;
            if (type == typeof(ApplicationStateManager)) return StateManager as T;
            if (type == typeof(ThemeManager)) return ThemeManager as T;
            if (type == typeof(ImportExportManager)) return ImportExportManager as T;
            if (type == typeof(IConfigurationManager)) return ConfigurationManager as T;
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered");
        }
    }
}