using System.Windows;
using System.Windows.Controls;
using AccountManager.ViewModels;

namespace AccountManager.Views.Window.Navigation
{
    public partial class Sidebar : UserControl
    {
        public Sidebar()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get the MainViewModel from the parent window's DataContext
            // Use System.Windows.Window to avoid namespace conflict
            if (System.Windows.Window.GetWindow(this)?.DataContext is MainViewModel mainViewModel)
            {
                DataContext = mainViewModel.SidebarViewModel;
                
                // Set up the group types for each SystemGroupsList
                TopSystemGroupsList.SetGroupType(GroupDisplayType.TopSystem);
                RegularGroupsList.SetGroupType(GroupDisplayType.Regular);
                BottomSystemGroupsList.SetGroupType(GroupDisplayType.BottomSystem);
            }
        }
    }
}