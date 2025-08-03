using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Views.Dialogs;

namespace AccountManager
{
    public static class DialogService
    {
        private static MainWindow _mainWindow;
        private static Grid _overlay;
        private static ContentPresenter _content;

        public static void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _overlay = mainWindow.FindName("DialogOverlay") as Grid;
            _content = mainWindow.FindName("DialogContent") as ContentPresenter;
        }

        public static async System.Threading.Tasks.Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            if (_overlay == null || _content == null)
                return false;

            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool?>();
            
            // Set up dialog content
            _content.Content = dialog;
            _overlay.Visibility = Visibility.Visible;

            // Handle dialog result
            if (dialog is GroupDialog groupDialog)
            {
                SetupGroupDialogHandlers(groupDialog, tcs);
            }
            else if (dialog is AccountDialog accountDialog)
            {
                SetupAccountDialogHandlers(accountDialog, tcs);
            }
            else if (dialog is SettingsDialog settingsDialog)
            {
                SetupSettingsDialogHandlers(settingsDialog, tcs);
            }

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
            CloseDialog();

            return result;
        }

        private static void SetupGroupDialogHandlers(GroupDialog dialog, System.Threading.Tasks.TaskCompletionSource<bool?> tcs)
        {
            // Use a small delay to ensure the dialog is fully loaded
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, "Cancel");
                var saveButton = FindButtonByContent(dialog, dialog.ViewModel?.ActionButtonText ?? "Save Changes");

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                    System.Diagnostics.Debug.WriteLine("Cancel button handler attached");
                }

                if (saveButton != null)
                {
                    saveButton.Click += (s, e) => {
                        if (dialog.ViewModel?.CanSave == true)
                        {
                            tcs.TrySetResult(true);
                        }
                    };
                    System.Diagnostics.Debug.WriteLine("Save button handler attached");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void SetupAccountDialogHandlers(AccountDialog dialog, System.Threading.Tasks.TaskCompletionSource<bool?> tcs)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
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

        private static void SetupSettingsDialogHandlers(SettingsDialog dialog, System.Threading.Tasks.TaskCompletionSource<bool?> tcs)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, "Cancel");

                if (cancelButton != null)
                {
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);
                    System.Diagnostics.Debug.WriteLine("Settings Cancel button handler attached");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static Button FindButtonByContent(DependencyObject parent, string content)
        {
            if (parent == null || string.IsNullOrEmpty(content)) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    var buttonContent = button.Content?.ToString();
                    if (buttonContent == content)
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

        public static void CloseDialog()
        {
            if (_overlay != null && _content != null)
            {
                _overlay.Visibility = Visibility.Collapsed;
                _content.Content = null;
            }
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