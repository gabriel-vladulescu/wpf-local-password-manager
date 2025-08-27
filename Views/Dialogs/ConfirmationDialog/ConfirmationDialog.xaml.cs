using System;
using System.Windows.Controls;
using AccountManager.Models;
using AccountManager.ViewModels;

namespace AccountManager.Views.Dialogs
{
    public partial class ConfirmationDialog : UserControl
    {
        public bool? DialogResult { get; set; }
        public event EventHandler DialogClosed;

        public ConfirmationDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ConfirmationDialogViewModel oldViewModel)
            {
                oldViewModel.CloseRequested -= OnCloseRequested;
            }

            if (e.NewValue is ConfirmationDialogViewModel newViewModel)
            {
                newViewModel.CloseRequested += OnCloseRequested;
            }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                DialogResult = ViewModel.Result;
            }
            DialogClosed?.Invoke(this, EventArgs.Empty);
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
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupForAccountTrash(account);
            DataContext = viewModel;
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

        public void SetupForAccountArchive(Account account)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupForAccountArchive(account);
            DataContext = viewModel;
        }

        // DON'T ADD: Any WireUpCommands method
        // DON'T ADD: Any RelayCommand code
        // DON'T ADD: Any command override code
    }
}