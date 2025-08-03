using System;
using AccountManager.Services;
using AccountManager.Services.Interfaces;
using AccountManager.ViewModels;

namespace AccountManager.Configuration
{
    /// <summary>
    /// Simple dependency injection container for the application
    /// </summary>
    public class ServiceContainer
    {
        private static ServiceContainer _instance;
        public static ServiceContainer Instance => _instance ??= new ServiceContainer();

        private readonly System.Collections.Generic.Dictionary<Type, object> _services;
        private readonly System.Collections.Generic.Dictionary<Type, Func<object>> _factories;

        private ServiceContainer()
        {
            _services = new System.Collections.Generic.Dictionary<Type, object>();
            _factories = new System.Collections.Generic.Dictionary<Type, Func<object>>();
            RegisterDefaultServices();
        }

        /// <summary>
        /// Register a singleton service instance
        /// </summary>
        public void RegisterSingleton<TInterface, TImplementation>(TImplementation instance)
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>
        /// Register a transient service factory
        /// </summary>
        public void RegisterTransient<TInterface>(Func<TInterface> factory)
        {
            _factories[typeof(TInterface)] = () => factory() ?? throw new InvalidOperationException($"Factory for {typeof(TInterface).Name} returned null");
        }

        /// <summary>
        /// Register a transient service type
        /// </summary>
        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _factories[typeof(TInterface)] = () => new TImplementation();
        }

        /// <summary>
        /// Resolve a service
        /// </summary>
        public T Resolve<T>()
        {
            var type = typeof(T);
            
            // Check for singleton instance
            if (_services.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }
            
            // Check for factory
            if (_factories.TryGetValue(type, out var factory))
            {
                return (T)factory();
            }
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered");
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public bool IsRegistered<T>()
        {
            var type = typeof(T);
            return _services.ContainsKey(type) || _factories.ContainsKey(type);
        }

        /// <summary>
        /// Clear all registrations
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }

        /// <summary>
        /// Register default application services
        /// </summary>
        private void RegisterDefaultServices()
        {
            // Register singleton services
            RegisterSingleton<IDialogService, DialogService>(DialogService.Instance);
            RegisterSingleton<ISettingsService, SettingsService>(SettingsService.Instance);
            RegisterSingleton<IThemeService, ThemeService>(ThemeService.Instance);
            RegisterSingleton<IValidationService, ValidationService>(ValidationService.Instance);
            
            // Register transient services
            RegisterTransient<IJsonService, JsonService>();
            
            // Register ViewModels as transient
            RegisterTransient<MainViewModel>();
        }
    }

    /// <summary>
    /// Service locator pattern for accessing services
    /// </summary>
    public static class ServiceLocator
    {
        public static T Get<T>() => ServiceContainer.Instance.Resolve<T>();
        public static bool IsRegistered<T>() => ServiceContainer.Instance.IsRegistered<T>();
    }
}