using System;
using System.Windows;
using System.Windows.Controls;
using AccountManager.Views.Dialogs;
using AccountManager.Utilities.Helpers;

namespace AccountManager.Views.Window.Controls
{
    public partial class SettingsButton : UserControl
    {
        public SettingsButton()
        {
            InitializeComponent();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SettingsDialog();
                dialog.SetupDialog();
                await DialogHelper.ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing settings: {ex.Message}");
            }
        }
    }
}