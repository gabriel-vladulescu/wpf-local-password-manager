using AccountManager.Views;
using System.Windows.Controls;

namespace AccountManager.Views.Dialogs
{
    /// <summary>
    /// Unified dialog for creating and editing accounts
    /// Handles both create and edit modes through the AccountDialogViewModel
    /// </summary>
    public partial class AccountDialog : UserControl
    {
        public AccountDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get the view model for this dialog
        /// </summary>
        public AccountDialogViewModel ViewModel => DataContext as AccountDialogViewModel;

        /// <summary>
        /// Setup the dialog for create mode
        /// </summary>
        public void SetupForCreate()
        {
            var viewModel = new AccountDialogViewModel();
            viewModel.InitializeForCreate();
            DataContext = viewModel;
        }

        /// <summary>
        /// Setup the dialog for edit mode
        /// </summary>
        public void SetupForEdit(Models.Account account)
        {
            var viewModel = new AccountDialogViewModel();
            viewModel.InitializeForEdit(account);
            DataContext = viewModel;
        }
    }
}