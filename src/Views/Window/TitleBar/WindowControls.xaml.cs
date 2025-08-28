using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Views.Window.TitleBar
{
    public partial class WindowControls : UserControl
    {
        public WindowControls()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Window.GetWindow(this);
            if (window?.WindowState != WindowState.Minimized)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            HandleMaximizeRestore();
        }

        public void HandleMaximizeRestore()
        {
            var mainWindow = System.Windows.Window.GetWindow(this) as Views.Window.MainWindow;
            if (mainWindow == null) return;

            var fadeOut = CreateFadeAnimation(1.0, 0.9, 100);
            var fadeIn = CreateFadeAnimation(0.9, 1.0, 100);

            fadeOut.Completed += (s, args) =>
            {
                mainWindow.ToggleMaximizeRestore();
                UpdateMaximizeRestoreButton();
                mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = System.Windows.Window.GetWindow(this);
            if (window == null) return;

            var fadeOut = CreateFadeAnimation(1.0, 0.0, 200);
            fadeOut.Completed += (s, args) => window.Close();
            window.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        
        private void UpdateMaximizeRestoreButton()
        {
            var mainWindow = System.Windows.Window.GetWindow(this) as Views.Window.MainWindow;
            if (mainWindow == null) return;

            if (mainWindow.IsMaximizedToWorkingArea)
            {
                MaximizeRestoreIcon.Kind = PackIconKind.WindowRestore;
                MaximizeRestoreButton.ToolTip = "Restore";
            }
            else
            {
                MaximizeRestoreIcon.Kind = PackIconKind.WindowMaximize;
                MaximizeRestoreButton.ToolTip = "Maximize";
            }
        }

        private DoubleAnimation CreateFadeAnimation(double from, double to, int durationMs)
        {
            return new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = to > from ? EasingMode.EaseIn : EasingMode.EaseOut }
            };
        }
    }
}