using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Views.Dialogs;
using AccountManager.Views.Window;
using AccountManager.Views.Window.Content;

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
            
            var appLayout = FindVisualChild<AppLayout>(mainWindow);
            if (appLayout != null)
            {
                _overlay = appLayout.FindName("DialogOverlayGrid") as Grid;
                _content = appLayout.FindName("DialogContent") as ContentPresenter;
            }
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

        public static async Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            if (_overlay == null || _content == null)
                return false;

            var tcs = new TaskCompletionSource<bool?>();
            
            _content.Content = dialog;
            _overlay.Visibility = Visibility.Visible;

            SetupDialogHandlers(dialog, tcs);
            SetupOverlayClickHandler(tcs);

            var result = await tcs.Task;
            CloseDialog();
            return result;
        }

        private static void SetupDialogHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs)
        {
            switch (dialog)
            {
                case GroupDialog groupDialog:
                    SetupButtonHandlers(groupDialog, tcs, groupDialog.ViewModel?.ActionButtonText ?? "Save Changes");
                    break;
                case AccountDialog accountDialog:
                    SetupButtonHandlers(accountDialog, tcs, accountDialog.ViewModel?.ActionButtonText);
                    break;
                case ConfirmationDialog confirmDialog:
                    confirmDialog.DialogClosed += (s, e) => tcs.TrySetResult(confirmDialog.DialogResult);
                    break;
                case SettingsDialog settingsDialog:
                    SetupButtonHandlers(settingsDialog, tcs, null);
                    break;
            }
        }

        private static void SetupButtonHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs, string saveButtonText)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var cancelButton = FindButtonByContent(dialog, "Cancel");
                var saveButton = saveButtonText != null ? FindButtonByContent(dialog, saveButtonText) : null;

                if (cancelButton != null)
                    cancelButton.Click += (s, e) => tcs.TrySetResult(false);

                if (saveButton != null)
                {
                    saveButton.Click += (s, e) => {
                        var canSave = dialog switch
                        {
                            GroupDialog gd => gd.ViewModel?.CanSave == true,
                            AccountDialog ad => ad.ViewModel?.CanSave == true,
                            _ => true
                        };
                        
                        if (canSave)
                            tcs.TrySetResult(true);
                    };
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void SetupOverlayClickHandler(TaskCompletionSource<bool?> tcs)
        {
            var overlayHandler = new MouseButtonEventHandler((s, e) => {
                if (e.OriginalSource == _overlay)
                    tcs.TrySetResult(false);
            });
            
            _overlay.MouseLeftButtonDown += overlayHandler;
            
            // Remove handler when task completes
            tcs.Task.ContinueWith(_ => _overlay.MouseLeftButtonDown -= overlayHandler);
        }

        private static Button FindButtonByContent(DependencyObject parent, string content)
        {
            if (parent == null || string.IsNullOrEmpty(content)) 
                return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is Button button && button.Content?.ToString() == content)
                    return button;

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