using System;
using System.Windows;
using System.Windows.Controls;
using AccountManager.Views.Base;

namespace AccountManager.Views.Base
{
    /// <summary>
    /// Base class for all dialog user controls
    /// </summary>
    public abstract class BaseDialog : UserControl
    {
        protected BaseDialog()
        {
            Loaded += OnDialogLoaded;
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus the first input field when dialog opens
            var firstInput = DialogHelper.FindFirstInputControl(this);
            if (firstInput != null)
            {
                // Use Dispatcher to ensure UI is fully loaded
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DialogHelper.SetFocusWithRetry(firstInput);
                    
                    // Select all text if in edit mode for easy overwriting
                    if (firstInput is TextBox textBox && 
                        DataContext is BaseDialogViewModel vm && vm.IsEditMode)
                    {
                        textBox.SelectAll();
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }

            // Setup dialog-specific behavior
            SetupDialogBehavior();
        }

        /// <summary>
        /// Virtual method that can be overridden to set up dialog-specific behavior
        /// </summary>
        protected virtual void SetupDialogBehavior() 
        {
            // Override in derived classes for specific behavior
        }

        /// <summary>
        /// Handle key events for common dialog shortcuts
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Handle Escape key to close dialog
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (DataContext is BaseDialogViewModel vm && vm.CancelCommand?.CanExecute(null) == true)
                {
                    vm.CancelCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}