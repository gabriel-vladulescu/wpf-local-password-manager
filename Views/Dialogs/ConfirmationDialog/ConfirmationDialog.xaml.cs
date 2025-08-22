using System;
using System.Windows.Controls;
using AccountManager.Models;
using AccountManager.ViewModels;

namespace AccountManager.Views.Dialogs
{
    public partial class ConfirmationDialog : UserControl
    {
        // ADD these two properties:
        public bool? DialogResult { get; set; }
        public event EventHandler DialogClosed;

        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        public ConfirmationDialogViewModel ViewModel => DataContext as ConfirmationDialogViewModel;

        // Your existing methods (keep these):
        public void SetupGroupDeleteConfirmation(AccountGroup group)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupGroupDeleteConfirmation(group);
            DataContext = viewModel;
        }

        public void SetupAccountDeleteConfirmation(Account account)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupAccountDeleteConfirmation(account);
            DataContext = viewModel;
        }

        public void SetupAccountEditConfirmation(Account account)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupAccountEditConfirmation(account);
            DataContext = viewModel;
        }

        public void RequestClose()
        {
            // Implementation depends on your dialog service
        }

        // ADD these new methods (simple versions):
        public void SetupForAccountTrash(Account account)
        {
            SetupAccountDeleteConfirmation(account); // Reuse existing
        }

        public void SetupForEmptyTrash(int itemCount)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupForEmptyTrash(itemCount);
            DataContext = viewModel;
        }

        public void SetupForGroupDelete(AccountGroup group)
        {
            SetupGroupDeleteConfirmation(group); // Reuse existing
        }

        public void SetupForAccountDelete(Account account)
        {
            SetupAccountDeleteConfirmation(account); // Reuse existing
        }

        // DON'T ADD: Any WireUpCommands method
        // DON'T ADD: Any RelayCommand code
        // DON'T ADD: Any command override code
    }
}