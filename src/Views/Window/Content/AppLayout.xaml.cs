using System.Windows;
using System.Windows.Controls;
using AccountManager.Views.Components;

namespace AccountManager.Views.Window.Content
{
    public partial class AppLayout : UserControl
    {
        public AppLayout()
        {
            InitializeComponent();
        }

        public ToastContainer GetToastContainer()
        {
            return ToastContainer;
        }
    }
}