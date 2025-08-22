using System.Collections.Generic;
using System.Windows.Controls;
using AccountManager.Models;
using AccountManager.ViewModels;

namespace AccountManager.Views.Dialogs
{
    public partial class TrashMigrationDialog : UserControl
    {
        public TrashMigrationDialog()
        {
            InitializeComponent();
        }

        public TrashMigrationDialogViewModel ViewModel => DataContext as TrashMigrationDialogViewModel;

        public void SetupDialog(List<Account> itemsToMigrate, List<AccountGroup> availableGroups)
        {
            var viewModel = new TrashMigrationDialogViewModel();
            viewModel.SetupDialog(itemsToMigrate, availableGroups);
            DataContext = viewModel;
        }
    }
}