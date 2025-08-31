using System;
using System.Windows.Controls;
using AccountManager.ViewModels;

namespace AccountManager.Views.Dialogs
{
    public partial class PassphraseDialog : UserControl
    {
        public bool? DialogResult { get; set; }
        public event EventHandler DialogClosed;

        public PassphraseDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PassphraseDialogViewModel oldViewModel)
            {
                oldViewModel.CloseRequested -= OnCloseRequested;
            }

            if (e.NewValue is PassphraseDialogViewModel newViewModel)
            {
                newViewModel.CloseRequested += OnCloseRequested;
            }
        }

        private void OnCloseRequested(object sender, bool result)
        {
            System.Diagnostics.Debug.WriteLine($"PassphraseDialog: OnCloseRequested called with result = {result}");
            DialogResult = result;
            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private void OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PassphraseDialogViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Passphrase = passwordBox.Password;
            }
        }

        private void OnConfirmPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PassphraseDialogViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmPassphrase = passwordBox.Password;
            }
        }

        public PassphraseDialogViewModel ViewModel => DataContext as PassphraseDialogViewModel;

        public void SetupForNewEncryption()
        {
            var viewModel = new PassphraseDialogViewModel();
            viewModel.SetupForNewEncryption();
            DataContext = viewModel;
        }

        public void SetupForExistingEncryption()
        {
            System.Diagnostics.Debug.WriteLine("PassphraseDialog: SetupForExistingEncryption called");
            try
            {
                var viewModel = new PassphraseDialogViewModel();
                System.Diagnostics.Debug.WriteLine("PassphraseDialog: ViewModel created");
                
                viewModel.SetupForExistingEncryption();
                System.Diagnostics.Debug.WriteLine("PassphraseDialog: ViewModel setup completed");
                
                DataContext = viewModel;
                System.Diagnostics.Debug.WriteLine("PassphraseDialog: DataContext set");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PassphraseDialog: Setup exception: {ex}");
            }
        }

        public string GetPassphrase()
        {
            return PassphraseBox.Password;
        }

        public void ClearPasswords()
        {
            PassphraseBox.Clear();
            ConfirmPassphraseBox.Clear();
        }
    }
}