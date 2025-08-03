using AccountManager.Views;
using System.Windows.Controls;
using AccountManager.Views.Dialogs; // Add this for the dialog classes
using AccountManager.ViewModels;     // This should already be there

namespace AccountManager.Views.Dialogs
{
    public partial class SettingsDialog : UserControl
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public SettingsDialogViewModel ViewModel => DataContext as SettingsDialogViewModel;

        public void SetupDialog()
        {
            var viewModel = new SettingsDialogViewModel();
            viewModel.InitializeForView();
            DataContext = viewModel;
        }
    }
}