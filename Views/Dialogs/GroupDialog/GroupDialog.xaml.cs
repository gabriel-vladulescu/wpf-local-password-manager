using AccountManager.Views;
using System.Windows.Controls;
using AccountManager.Views.Dialogs; // Add this for the dialog classes
using AccountManager.ViewModels;     // This should already be there

namespace AccountManager.Views.Dialogs
{
    /// <summary>
    /// Unified dialog for creating and editing groups
    /// Handles both create and edit modes through the GroupDialogViewModel
    /// </summary>
    public partial class GroupDialog : UserControl
    {
        public GroupDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get the view model for this dialog
        /// </summary>
        public GroupDialogViewModel ViewModel => DataContext as GroupDialogViewModel;

        /// <summary>
        /// Setup the dialog for create mode
        /// </summary>
        public void SetupForCreate()
        {
            var viewModel = new GroupDialogViewModel();
            viewModel.InitializeForCreate();
            DataContext = viewModel;
        }

        /// <summary>
        /// Setup the dialog for edit mode
        /// </summary>
        public void SetupForEdit(Models.AccountGroup group)
        {
            var viewModel = new GroupDialogViewModel();
            viewModel.InitializeForEdit(group);
            DataContext = viewModel;
        }
    }
}