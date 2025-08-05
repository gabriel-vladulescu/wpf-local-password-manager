using System;
using System.Collections.Generic;
using System.Windows.Controls;
using AccountManager.Models;

namespace AccountManager.Views.Dialogs
{
    public partial class ConfirmationDialog : UserControl
    {
        public bool? DialogResult { get; private set; }
        public event EventHandler DialogClosed;
        public ConfirmationDialogViewModel ViewModel => DataContext as ConfirmationDialogViewModel;

        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        public void SetupForGroupDelete(AccountGroup group)
        {
            SetupDialog(vm => vm.SetupGroupDeleteConfirmation(group));
        }

        public void SetupForAccountDelete(Account account)
        {
            SetupDialog(vm => vm.SetupAccountDeleteConfirmation(account));
        }

        public void SetupForAccountEdit(List<string> changes)
        {
            SetupDialog(vm => vm.SetupAccountEditConfirmation(changes));
        }

        private void SetupDialog(Action<ConfirmationDialogViewModel> setupAction)
        {
            var viewModel = new ConfirmationDialogViewModel();
            setupAction(viewModel);
            
            viewModel.RequestClose += (sender, result) => {
                DialogResult = result;
                DialogClosed?.Invoke(this, EventArgs.Empty);
            };
            
            DataContext = viewModel;
        }
    }
}