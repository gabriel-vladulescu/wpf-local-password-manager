using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AccountManager.Views.Base;

namespace AccountManager.Utilities.Extensions
{
    public static class DialogExtensions
    {
        /// <summary>
        /// Show a dialog and handle common setup
        /// </summary>
        public static async Task<bool?> ShowWithSetupAsync(this BaseDialog dialog)
        {
            dialog.SetupDialogBehavior();
            return await DialogHelper.ShowDialogAsync(dialog);
        }

        /// <summary>
        /// Show a dialog with error handling
        /// </summary>
        public static async Task<bool?> ShowSafelyAsync(this BaseDialog dialog, string errorTitle = "Dialog Error")
        {
            return await DialogHelper.ShowDialogWithErrorHandlingAsync(dialog, errorTitle);
        }

        /// <summary>
        /// Setup a dialog for create mode
        /// </summary>
        public static T SetupForCreate<T>(this T dialog) where T : BaseDialog
        {
            if (dialog.DataContext is BaseDialogViewModel vm)
            {
                // This would need to be implemented per dialog type
                // or we could use reflection or a common interface
            }
            dialog.SetupDialogBehavior();
            return dialog;
        }

        /// <summary>
        /// Setup a dialog for edit mode
        /// </summary>
        public static T SetupForEdit<T, TModel>(this T dialog, TModel model) where T : BaseDialog
        {
            if (dialog.DataContext is BaseDialogViewModel vm)
            {
                // This would need to be implemented per dialog type
                // or we could use reflection or a common interface
            }
            dialog.SetupDialogBehavior();
            return dialog;
        }
    }
}