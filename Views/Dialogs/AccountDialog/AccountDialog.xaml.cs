using AccountManager.Views;
using System.Windows.Controls;

namespace AccountManager.Views.Dialogs
{
    public partial class AccountDialog : UserControl
    {
        public AccountDialog()
        {
            InitializeComponent();
        }

        public AccountDialogViewModel ViewModel => DataContext as AccountDialogViewModel;

        public void SetupForCreate()
        {
            var viewModel = new AccountDialogViewModel();
            viewModel.InitializeForCreate();
            DataContext = viewModel;
        }

        public void SetupForEdit(Models.Account account)
        {
            var viewModel = new AccountDialogViewModel();
            viewModel.InitializeForEdit(account);
            DataContext = viewModel;
        }
    }
}