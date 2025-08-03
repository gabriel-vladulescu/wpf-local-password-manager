using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Models;
using AccountManager.Views.Dialogs; // Add this for the dialog classes
using AccountManager.ViewModels;     // This should already be there

namespace AccountManager.Views.Dialogs
{
    public partial class ConfirmationDialog : UserControl
    {
        public bool? DialogResult { get; private set; }
        public event EventHandler DialogClosed;

        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        public ConfirmationDialogViewModel ViewModel => DataContext as ConfirmationDialogViewModel;

        public void SetupForGroupDelete(AccountGroup group)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupGroupDeleteConfirmation(group);
            
            // Subscribe to the close request event
            viewModel.RequestClose += (sender, result) => {
                DialogResult = result;
                DialogClosed?.Invoke(this, EventArgs.Empty);
            };
            
            DataContext = viewModel;
        }

        public void SetupForAccountDelete(Account account)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupAccountDeleteConfirmation(account);
            
            // Subscribe to the close request event
            viewModel.RequestClose += (sender, result) => {
                DialogResult = result;
                DialogClosed?.Invoke(this, EventArgs.Empty);
            };
            
            DataContext = viewModel;
        }

        public void SetupForAccountEdit(List<string> changes)
        {
            var viewModel = new ConfirmationDialogViewModel();
            viewModel.SetupAccountEditConfirmation(changes);
            
            // Subscribe to the close request event
            viewModel.RequestClose += (sender, result) => {
                DialogResult = result;
                DialogClosed?.Invoke(this, EventArgs.Empty);
            };
            
            DataContext = viewModel;
        }
    }
}