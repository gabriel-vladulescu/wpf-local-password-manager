using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Services.Interfaces;
using AccountManager.Views.Dialogs;

namespace AccountManager.Services
{
    public class DialogService : IDialogService
    {
        private static DialogService _instance;
        public static DialogService Instance => _instance ??= new DialogService();

        private Window _mainWindow;
        private Grid _overlay;
        private ContentPresenter _content;

        private DialogService() { }

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _overlay = mainWindow.FindName("DialogOverlay") as Grid;
            _content = mainWindow.FindName("DialogContent") as ContentPresenter;

            if (_overlay == null || _content == null)
            {
                throw new InvalidOperationException("Dialog overlay or content not found in main window");
            }
        }

        public async Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            if (_overlay == null || _content == null)
            {
                throw new InvalidOperationException("DialogService not initialized. Call Initialize() first.");
            }

            if (dialog == null)
                throw new ArgumentNullException(nameof(dialog));

            var tcs = new TaskCompletionSource<bool?>();
            
            try
            {
                // Set up dialog content
                _content.Content = dialog;
                _overlay.Visibility = Visibility.Visible;

                // Handle dialog result based on type
                SetupDialogHandlers(dialog, tcs);

                // Handle overlay click to close
                var overlayHandler = new MouseButtonEventHandler((s, e) => {
                    if (e.OriginalSource == _overlay)
                    {
                        CloseDialog();
                        tcs.TrySetResult(false);
                    }
                });
                _overlay.MouseLeftButtonDown += overlayHandler;

                var result = await tcs.Task;

                // Cleanup
                _overlay.MouseLeftButtonDown -= overlayHandler;
                
                return result;
            }
            finally
            {
                CloseDialog();
            }
        }

        public async Task<bool?> ShowDialogWithErrorHandlingAsync(UserControl dialog, string errorTitle = "Dialog Error", string errorMessage = null)
        {
            try
            {
                return await ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                var message = errorMessage ?? $"An error occurred while showing the dialog: {ex.Message}";
                MessageBox.Show(message, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void CloseDialog()
        {
            if (_overlay != null && _content != null)
            {
                _overlay.Visibility = Visibility.Collapsed;
                _content.Content = null;
            }
        }

        private void SetupDialogHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs)
        {
            switch (dialog)
            {
                case GroupDialog groupDialog:
                    SetupGroupDialogHandlers(groupDialog, tcs);
                    break;
                case AccountDialog accountDialog:
                    SetupAccountDialogHandlers(accountDialog, tcs);
                    break;
                case SettingsDialog settingsDialog:
                    SetupSettingsDialogHandlers(settingsDialog, tcs);
                    break;
                case ConfirmationDialog confirmationDialog:
                    SetupConfirmationDialogHandlers(confirmationDialog, tcs);
                    break;
                default:
                    // Generic handler for unknown dialog types
                    SetupGenericDialogHandlers(dialog, tcs);
                    break;
            }
        }

        private void SetupGroupDialogHandlers(GroupDialog dialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, "Cancel");
                var saveButton = FindButtonByContent(dialog, dialog.ViewModel?.ActionButtonText ?? "Save Changes");

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                }

                if (saveButton != null)
                {
                    saveButton.Click += (s, e) => {
                        if (dialog.ViewModel?.CanSave == true)
                        {
                            tcs.TrySetResult(true);
                        }
                    };
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetupAccountDialogHandlers(AccountDialog dialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, "Cancel");
                var saveButton = FindButtonByContent(dialog, dialog.ViewModel?.ActionButtonText);

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                }

                if (saveButton != null)
                {
                    saveButton.Click += (s, e) => {
                        if (dialog.ViewModel?.CanSave == true)
                        {
                            tcs.TrySetResult(true);
                        }
                    };
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetupSettingsDialogHandlers(SettingsDialog dialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var closeButton = FindButtonByContent(dialog, "Close") ?? FindButtonByContent(dialog, "Cancel");

                if (closeButton != null)
                {
                    closeButton.Click += (s, e) => tcs.TrySetResult(true); // Settings always save automatically
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetupConfirmationDialogHandlers(ConfirmationDialog dialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, dialog.ViewModel?.CancelButtonText ?? "Cancel");
                var confirmButton = FindButtonByContent(dialog, dialog.ViewModel?.ConfirmButtonText ?? "Confirm");

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                }

                if (confirmButton != null)
                {
                    confirmButton.Click += (s, e) => {
                        if (dialog.ViewModel?.CanSave == true)
                        {
                            tcs.TrySetResult(true);
                        }
                    };
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetupGenericDialogHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Try to find common button names
                var okButton = FindButtonByContent(dialog, "OK");
                var cancelButton = FindButtonByContent(dialog, "Cancel");
                var closeButton = FindButtonByContent(dialog, "Close");

                if (okButton != null)
                {
                    okButton.Click += (s, e) => tcs.TrySetResult(true);
                }

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                }

                if (closeButton != null)
                {
                    closeButton.Click += (s, e) => tcs.TrySetResult(null);
                }

                // If no buttons found, close after a timeout
                if (okButton == null && cancelButton == null && closeButton == null)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(30)
                    };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        tcs.TrySetResult(null);
                    };
                    timer.Start();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private Button FindButtonByContent(DependencyObject parent, string content)
        {
            if (parent == null || string.IsNullOrEmpty(content)) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    var buttonContent = button.Content?.ToString();
                    if (string.Equals(buttonContent, content, StringComparison.OrdinalIgnoreCase))
                    {
                        return button;
                    }
                }

                var result = FindButtonByContent(child, content);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    public class EmptyCommand : ICommand
    {
        public static readonly EmptyCommand Instance = new EmptyCommand();

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) { }
    }
}