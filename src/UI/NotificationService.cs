using System.Windows;
using AccountManager.Core.Interfaces;
using AccountManager.Views.Components;
using AccountManager.Views.Window.Content;
using AccountManager.Core;

namespace AccountManager.UI
{
    /// <summary>
    /// Handles user notifications and messages with custom toast notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private static NotificationService _instance;
        private IConfigurationManager _configurationManager;
        
        public static NotificationService Instance => _instance ??= new NotificationService();

        private NotificationService() 
        {
            // Don't initialize _configurationManager here to avoid circular dependency
        }

        private IConfigurationManager GetConfigurationManager()
        {
            if (_configurationManager == null)
            {
                _configurationManager = ServiceContainer.Instance.GetService<IConfigurationManager>();
            }
            return _configurationManager;
        }

        private bool AreNotificationsEnabled()
        {
            var configManager = GetConfigurationManager();
            return configManager?.EnableApplicationNotifications ?? true;
        }

        private ToastContainer GetToastContainer()
        {
            if (Application.Current?.MainWindow is Views.Window.MainWindow mainWindow)
            {
                // Find AppLayout in the visual tree
                var appLayout = FindVisualChild<AppLayout>(mainWindow);
                return appLayout?.GetToastContainer();
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is T result)
                    return result;
                
                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        public void ShowInfo(string message, string title = "Information")
        {
            if (!AreNotificationsEnabled()) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var toastContainer = GetToastContainer();
                if (toastContainer != null)
                {
                    toastContainer.ShowToast(title, message, ToastNotification.ToastType.Info);
                }
                else
                {
                    // Fallback to MessageBox if toast container not found
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        public void ShowError(string message, string title = "Error")
        {
            // Error messages always show regardless of notification settings
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toastContainer = GetToastContainer();
                if (toastContainer != null)
                {
                    // Error toasts don't auto-close, must be manually dismissed
                    toastContainer.ShowToast(title, message, ToastNotification.ToastType.Error, false);
                }
                else
                {
                    // Fallback to MessageBox if toast container not found
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            if (!AreNotificationsEnabled()) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var toastContainer = GetToastContainer();
                if (toastContainer != null)
                {
                    toastContainer.ShowToast(title, message, ToastNotification.ToastType.Warning);
                }
                else
                {
                    // Fallback to MessageBox if toast container not found
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            if (!AreNotificationsEnabled()) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var toastContainer = GetToastContainer();
                if (toastContainer != null)
                {
                    toastContainer.ShowToast(title, message, ToastNotification.ToastType.Success);
                }
                else
                {
                    // Fallback to MessageBox if toast container not found
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}