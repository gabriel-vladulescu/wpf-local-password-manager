using System.Collections.Generic;
using System.Windows.Controls;
using AccountManager.Models;
using AccountManager.ViewModels;

namespace AccountManager.Views.Dialogs
{
    public partial class ArchiveMigrationDialog : UserControl
    {
        public ArchiveMigrationDialog()
        {
            InitializeComponent();
        }

        public ArchiveMigrationDialogViewModel ViewModel => DataContext as ArchiveMigrationDialogViewModel;

        public void SetupDialog(List<Account> itemsToMigrate, List<AccountGroup> availableGroups)
        {
            var viewModel = new ArchiveMigrationDialogViewModel();
            viewModel.SetupDialog(itemsToMigrate, availableGroups);
            DataContext = viewModel;
        }
    }
}