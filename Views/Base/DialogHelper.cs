using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccountManager.Services;

namespace AccountManager.Views.Base
{
    /// <summary>
    /// Helper methods for dialog operations
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Find the first visible and enabled TextBox in a visual tree
        /// </summary>
        /// <param name="parent">The parent element to search</param>
        /// <returns>The first TextBox found, or null if none found</returns>
        public static TextBox FindFirstTextBox(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox && textBox.IsVisible && textBox.IsEnabled && textBox.Focusable)
                {
                    return textBox;
                }

                var result = FindFirstTextBox(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Find the first visible and enabled PasswordBox in a visual tree
        /// </summary>
        /// <param name="parent">The parent element to search</param>
        /// <returns>The first PasswordBox found, or null if none found</returns>
        public static PasswordBox FindFirstPasswordBox(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is PasswordBox passwordBox && passwordBox.IsVisible && passwordBox.IsEnabled && passwordBox.Focusable)
                {
                    return passwordBox;
                }

                var result = FindFirstPasswordBox(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Find the first focusable input control (TextBox or PasswordBox) in a visual tree
        /// </summary>
        /// <param name="parent">The parent element to search</param>
        /// <returns>The first input control found, or null if none found</returns>
        public static Control FindFirstInputControl(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Control control && control.IsVisible && control.IsEnabled && control.Focusable)
                {
                    if (control is TextBox || control is PasswordBox)
                    {
                        return control;
                    }
                }

                var result = FindFirstInputControl(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Show a dialog and return the result using custom dialog service
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        /// <returns>Dialog result</returns>
        public static async Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            if (dialog == null)
                throw new ArgumentNullException(nameof(dialog));

            try
            {
                return await DialogService.ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
                
                // Log the error but don't rethrow - return false to indicate failure
                LogError($"Dialog error: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Show a dialog with error handling and user feedback
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        /// <param name="errorTitle">Title for error message</param>
        /// <param name="errorMessage">Custom error message (optional)</param>
        /// <returns>Dialog result</returns>
        public static async Task<bool?> ShowDialogWithErrorHandlingAsync(UserControl dialog, string errorTitle = "Dialog Error", string errorMessage = null)
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

        /// <summary>
        /// Set focus to a control with retry mechanism
        /// </summary>
        /// <param name="control">The control to focus</param>
        /// <param name="retryCount">Number of retries</param>
        public static void SetFocusWithRetry(Control control, int retryCount = 3)
        {
            if (control == null) return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < retryCount; i++)
                {
                    if (control.Focus())
                        break;

                    // Wait a bit and try again
                    System.Threading.Thread.Sleep(10);
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Apply focus behavior to a dialog
        /// </summary>
        /// <param name="dialog">The dialog to apply focus behavior to</param>
        /// <param name="selectAllOnEdit">Whether to select all text in edit mode</param>
        public static void ApplyFocusBehavior(BaseDialog dialog, bool selectAllOnEdit = true)
        {
            if (dialog == null) return;

            dialog.Loaded += (sender, e) =>
            {
                var firstInput = FindFirstInputControl(dialog);
                if (firstInput != null)
                {
                    SetFocusWithRetry(firstInput);

                    // Select all text if in edit mode and it's a TextBox
                    if (selectAllOnEdit && firstInput is TextBox textBox &&
                        dialog.DataContext is BaseDialogViewModel vm && vm.IsEditMode)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            textBox.SelectAll();
                        }), System.Windows.Threading.DispatcherPriority.Input);
                    }
                }
            };
        }

        /// <summary>
        /// Log an error (can be extended to use a proper logging framework)
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception details</param>
        private static void LogError(string message, Exception exception = null)
        {
            // For now, just write to debug output
            // In a real application, you'd use a logging framework like NLog, Serilog, etc.
            System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
            if (exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Exception: {exception}");
            }
        }
    }

    /// <summary>
    /// Extension methods for views and dialogs
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Setup common dialog behavior
        /// </summary>
        /// <param name="dialog">The dialog to setup</param>
        public static void SetupDialogBehavior(this BaseDialog dialog)
        {
            if (dialog == null) return;

            // Apply focus behavior
            DialogHelper.ApplyFocusBehavior(dialog);
        }

        /// <summary>
        /// Get the parent window of a control
        /// </summary>
        /// <param name="control">The control</param>
        /// <returns>Parent window or null if not found</returns>
        public static Window GetParentWindow(this FrameworkElement control)
        {
            var parent = control?.Parent;
            while (parent != null && !(parent is Window))
            {
                if (parent is FrameworkElement frameworkElement)
                    parent = frameworkElement.Parent;
                else
                    break;
            }
            return parent as Window;
        }

        /// <summary>
        /// Center a dialog within its parent window
        /// </summary>
        /// <param name="dialog">The dialog to center</param>
        public static void CenterInParent(this BaseDialog dialog)
        {
            if (dialog == null) return;

            var parentWindow = dialog.GetParentWindow();
            if (parentWindow != null)
            {
                dialog.Loaded += (sender, e) =>
                {
                    try
                    {
                        var dialogWidth = dialog.ActualWidth;
                        var dialogHeight = dialog.ActualHeight;
                        var parentWidth = parentWindow.ActualWidth;
                        var parentHeight = parentWindow.ActualHeight;

                        if (dialogWidth > 0 && dialogHeight > 0 && parentWidth > 0 && parentHeight > 0)
                        {
                            var left = (parentWidth - dialogWidth) / 2;
                            var top = (parentHeight - dialogHeight) / 2;

                            Canvas.SetLeft(dialog, Math.Max(0, left));
                            Canvas.SetTop(dialog, Math.Max(0, top));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error centering dialog: {ex.Message}");
                    }
                };
            }
        }
    }

    /// <summary>
    /// Dialog result helper for consistent dialog outcomes
    /// </summary>
    public static class DialogResult
    {
        public static readonly bool? OK = true;
        public static readonly bool? Cancel = false;
        public static readonly bool? None = null;

        /// <summary>
        /// Convert boolean to dialog result
        /// </summary>
        /// <param name="result">Boolean result</param>
        /// <returns>Dialog result</returns>
        public static bool? FromBoolean(bool result) => result;

        /// <summary>
        /// Check if result indicates success
        /// </summary>
        /// <param name="result">Dialog result to check</param>
        /// <returns>True if result indicates success</returns>
        public static bool IsSuccess(bool? result) => result == true;

        /// <summary>
        /// Check if result indicates cancellation
        /// </summary>
        /// <param name="result">Dialog result to check</param>
        /// <returns>True if result indicates cancellation</returns>
        public static bool IsCanceled(bool? result) => result == false;

        /// <summary>
        /// Check if result is undefined/none
        /// </summary>
        /// <param name="result">Dialog result to check</param>
        /// <returns>True if result is undefined</returns>
        public static bool IsNone(bool? result) => result == null;
    }
}