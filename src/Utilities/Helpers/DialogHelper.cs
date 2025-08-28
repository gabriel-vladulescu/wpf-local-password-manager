using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccountManager.Core;

namespace AccountManager.Utilities.Helpers
{
    /// <summary>
    /// Helper class for dialog operations and UI interactions
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Show a dialog and return the result using custom dialog service
        /// </summary>
        public static async Task<bool?> ShowDialogAsync(UserControl dialog)
        {
            try
            {
                var dialogManager = ServiceContainer.Instance.DialogManager;
                return await dialogManager.ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Show a dialog with error handling and optional fallback
        /// </summary>
        public static async Task<bool?> ShowDialogWithErrorHandlingAsync(UserControl dialog, string errorTitle = "Dialog Error", string errorMessage = null)
        {
            try
            {
                return await ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dialog error: {ex.Message}");
                
                var message = errorMessage ?? $"An error occurred while showing the dialog: {ex.Message}";
                MessageBox.Show(message, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                
                return false;
            }
        }

        /// <summary>
        /// Find the first visible and enabled TextBox in a visual tree
        /// </summary>
        public static TextBox FindFirstTextBox(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox && textBox.IsVisible && textBox.IsEnabled)
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
        /// Show a simple confirmation dialog
        /// </summary>
        public static MessageBoxResult ShowConfirmation(string message, string title = "Confirm", MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            return MessageBox.Show(message, title, buttons, MessageBoxImage.Question);
        }

        /// <summary>
        /// Show an error message dialog
        /// </summary>
        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show an information message dialog
        /// </summary>
        public static void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show a warning message dialog
        /// </summary>
        public static void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
