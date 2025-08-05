using AccountManager.Views;
using System.Windows.Controls;
using AccountManager.ViewModels;

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