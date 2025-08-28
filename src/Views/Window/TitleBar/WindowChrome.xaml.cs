using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccountManager.Views.Window.TitleBar
{
    public partial class WindowChrome : UserControl
    {
        public WindowChrome()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to maximize/restore - delegate to window controls
                var windowControls = FindWindowControls();
                windowControls?.HandleMaximizeRestore();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                // Single click to drag
                try
                {
                    var window = System.Windows.Window.GetWindow(this);
                    window?.DragMove();
                }
                catch
                {
                    // Ignore any exceptions from DragMove
                }
            }
        }

        private WindowControls FindWindowControls()
        {
            // Find the WindowControls component in the visual tree
            var parent = this.Parent;
            while (parent != null)
            {
                if (parent is Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is WindowControls controls)
                            return controls;
                    }
                }
                parent = (parent as FrameworkElement)?.Parent;
            }
            return null;
        }
    }
}