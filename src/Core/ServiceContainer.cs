using System;
using System.Threading.Tasks;
using AccountManager.Core.Interfaces;
using AccountManager.Infrastructure.Storage;
using AccountManager.Infrastructure.Serialization;
using AccountManager.Infrastructure.Configuration;
using AccountManager.Infrastructure.Security;
using AccountManager.Core.Services;
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
        public IEncryptionService EncryptionService { get; private set; }
        public IEncryptionConfigManager EncryptionConfigManager { get; private set; }
        public StartupEncryptionService StartupEncryptionService { get; private set; }
        
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
                EncryptionService = new EncryptionService();
                
                // Initialize infrastructure with dependencies
                PathProvider = new PathProvider(FileStorage);
                var baseSerializer = new JsonSerializer(FileStorage);
                
                // Initialize with base serializer first
                Serializer = baseSerializer;
                DataRepository = new AppDataRepository(Serializer, PathProvider);
                ConfigurationManager = new ConfigurationManager(DataRepository, null);
                
                // Create AppConfig-based encryption manager that detects from file content
                EncryptionConfigManager = new AppConfigEncryptionManager(PathProvider);
                
                // Create encrypted serializer wrapper
                var encryptedSerializer = new EncryptedJsonSerializer(baseSerializer, EncryptionService, EncryptionConfigManager);
                
                // Replace serializer in repository with encrypted version
                Serializer = encryptedSerializer;
                DataRepository = new AppDataRepository(Serializer, PathProvider);
                
                // Re-initialize configuration manager with new repository
                ConfigurationManager = new ConfigurationManager(DataRepository, null);
                
                // Initialize remaining managers
                ApplicationStateManager.Initialize(DataRepository);
                StateManager = ApplicationStateManager.Instance;
                ThemeManager = new ThemeManager(DataRepository);
                ImportExportManager = new ImportExportManager(DataRepository, DialogManager, NotificationService);
                
                // Initialize startup encryption service
                StartupEncryptionService = new StartupEncryptionService(EncryptionConfigManager, EncryptionService, DialogManager, NotificationService);

            }
            catch (Exception ex)
            {
                // Show critical error if notification service is available
                try
                {
                    NotificationService?.ShowError($"Critical error initializing application: {ex.Message}", "Initialization Error");
                }
                catch { } // Prevent recursive errors
                throw;
            }
        }

        /// <summary>
        /// Completes async initialization after core services are created
        /// </summary>
        public async Task CompleteInitializationAsync()
        {
            try
            {
                // Load custom data path configuration
                await PathProvider.LoadCustomDataPathAsync();
            }
            catch (Exception ex)
            {
                try
                {
                    NotificationService?.ShowError($"Error completing initialization: {ex.Message}", "Initialization Error");
                }
                catch { }
            }
        }

        /// <summary>
        /// Initializes the path provider with custom path from settings
        /// Called after configuration is loaded
        /// </summary>
        public async Task InitializeCustomPathAsync()
        {
            try
            {
                var customPath = ConfigurationManager.CustomDataPath;
                if (!string.IsNullOrEmpty(customPath))
                {
                    await PathProvider.SetCustomDataPathAsync(customPath);
                }
            }
            catch (Exception ex)
            {
                NotificationService?.ShowError($"Error setting custom data path: {ex.Message}", "Configuration Error");
            }
        }

        /// <summary>
        /// Gets the encrypted serializer for passphrase management
        /// </summary>
        public EncryptedJsonSerializer GetEncryptedSerializer()
        {
            return Serializer as EncryptedJsonSerializer;
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
            if (type == typeof(IEncryptionService)) return EncryptionService as T;
            if (type == typeof(IEncryptionConfigManager)) return EncryptionConfigManager as T;
            if (type == typeof(AppDataRepository)) return DataRepository as T;
            if (type == typeof(ApplicationStateManager)) return StateManager as T;
            if (type == typeof(ThemeManager)) return ThemeManager as T;
            if (type == typeof(ImportExportManager)) return ImportExportManager as T;
            if (type == typeof(IConfigurationManager)) return ConfigurationManager as T;
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered");
        }
    }
}