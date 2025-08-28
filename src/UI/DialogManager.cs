using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Core.Interfaces;
using AccountManager.Views.Window;
using AccountManager.Views.Dialogs;
using Microsoft.Win32;

namespace AccountManager.UI
{
    /// <summary>
    /// Manages application dialogs and file dialogs
    /// Replaces the old DialogService with better separation
    /// </summary>
    public class DialogManager : IDialogManager
    {
        private static DialogManager _instance;
        private MainWindow _mainWindow;
        private Grid _overlay;
        private ContentPresenter _content;

        public static DialogManager Instance => _instance ??= new DialogManager();

        private DialogManager() { }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            
            // Try to initialize immediately
            TryFindDialogComponents();
            
            // If not found, try again after the window is loaded
            if (_overlay == null || _content == null)
            {
                mainWindow.Loaded += (s, e) => TryFindDialogComponents();
                mainWindow.ContentRendered += (s, e) => TryFindDialogComponents();
            }
        }

        private void TryFindDialogComponents()
        {
            if (_mainWindow == null) return;
            
            // Find the dialog overlay components
            var appLayout = FindVisualChild<Views.Window.Content.AppLayout>(_mainWindow);
            if (appLayout != null)
            {
                _overlay = appLayout.FindName("DialogOverlayGrid") as Grid;
                _content = appLayout.FindName("DialogContent") as ContentPresenter;
            }
        }

        public async Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            // Try one more time to find components if they're not found
            if (_overlay == null || _content == null)
            {
                TryFindDialogComponents();
            }
            
            if (_overlay == null || _content == null)
            {
                return false;
            }

            try
            {
                var tcs = new TaskCompletionSource<bool?>();
                
                _content.Content = dialog;
                _overlay.Visibility = Visibility.Visible;

                SetupDialogHandlers(dialog, tcs);
                SetupOverlayClickHandler(tcs);

                var result = await tcs.Task;
                CloseDialog();
                return result;
            }
            catch (Exception)
            {
                CloseDialog();
                return null;
            }
        }

        private void SetupDialogHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs)
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
                    SetupSettingsDialogHandlers(settingsDialog, tcs);
                    break;
            }
        }

        private void SetupSettingsDialogHandlers(UserControl settingsDialog, TaskCompletionSource<bool?> tcs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var closeButton = FindButtonByContent(settingsDialog, "Close");
                var resetButton = FindButtonByContent(settingsDialog, "Reset to Defaults");

                if (closeButton != null)
                    closeButton.Click += (s, e) => tcs.TrySetResult(true);

                // Reset button doesn't close the dialog, just resets settings
                // No handler needed for reset button
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SetupButtonHandlers(UserControl dialog, TaskCompletionSource<bool?> tcs, string saveButtonText)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
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

        private void SetupOverlayClickHandler(TaskCompletionSource<bool?> tcs)
        {
            var overlayHandler = new System.Windows.Input.MouseButtonEventHandler((s, e) => {
                if (e.OriginalSource == _overlay)
                    tcs.TrySetResult(false);
            });
            
            _overlay.MouseLeftButtonDown += overlayHandler;
            
            // Remove handler when task completes
            tcs.Task.ContinueWith(_ => _overlay.MouseLeftButtonDown -= overlayHandler);
        }

        private Button FindButtonByContent(DependencyObject parent, string content)
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

        public void CloseDialog()
        {
            if (_overlay != null)
            {
                _overlay.Visibility = Visibility.Collapsed;
            }
            
            if (_content != null)
            {
                _content.Content = null;
            }
        }

        public string ShowSelectFileDialog(string title, string filter, string defaultExt = null)
        {
            var openDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(defaultExt))
            {
                openDialog.DefaultExt = defaultExt;
            }

            return openDialog.ShowDialog() == true ? openDialog.FileName : null;
        }

        public string ShowSaveFileDialog(string title, string filter, string defaultFileName = null, string defaultExt = null)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                saveDialog.FileName = defaultFileName;
            }

            if (!string.IsNullOrEmpty(defaultExt))
            {
                saveDialog.DefaultExt = defaultExt;
            }

            return saveDialog.ShowDialog() == true ? saveDialog.FileName : null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                {
                    return childResult;
                }
            }
            return null;
        }
    }
}