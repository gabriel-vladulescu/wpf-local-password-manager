using AccountManager.Views;
using System.Windows.Controls;

namespace AccountManager.Views.Dialogs
{
    public partial class GroupDialog : UserControl
    {
        public GroupDialog()
        {
            InitializeComponent();
        }

        public GroupDialogViewModel ViewModel => DataContext as GroupDialogViewModel;

        public void SetupForCreate()
        {
            var viewModel = new GroupDialogViewModel();
            viewModel.InitializeForCreate();
            DataContext = viewModel;
        }

        public void SetupForEdit(Models.AccountGroup group)
        {
            var viewModel = new GroupDialogViewModel();
            viewModel.InitializeForEdit(group);
            DataContext = viewModel;
        }
    }
}