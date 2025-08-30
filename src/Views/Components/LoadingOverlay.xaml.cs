using System.Windows;
using System.Windows.Controls;

namespace AccountManager.Views.Components
{
    /// <summary>
    /// Loading overlay component for displaying loading states with messages
    /// </summary>
    public partial class LoadingOverlay : UserControl
    {
        public LoadingOverlay()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => LoadingMessage.Text;
            set => LoadingMessage.Text = value;
        }

        public string ProgressText
        {
            get => ProgressInfo.Text;
            set
            {
                ProgressInfo.Text = value;
                ProgressInfo.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}