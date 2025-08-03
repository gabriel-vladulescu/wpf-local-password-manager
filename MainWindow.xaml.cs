using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using AccountManager.ViewModels;
using AccountManager.Views.Dialogs;
using MaterialDesignThemes.Wpf;

namespace AccountManager
{
    public partial class MainWindow : Window
    {
        private WindowState _previousWindowState = WindowState.Normal;
        private double _previousWidth;
        private double _previousHeight;
        private double _previousLeft;
        private double _previousTop;
        private bool _isFullscreen = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            // Store initial window dimensions
            _previousWidth = Width;
            _previousHeight = Height;
            _previousLeft = Left;
            _previousTop = Top;

            // Initialize dialog service
            DialogService.Initialize(this);
            
            // Update maximize/restore button icon based on window state
            UpdateMaximizeRestoreButton();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Hook into Windows messages to prevent manual resizing
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SIZING = 0x0214;
            const int WM_NCHITTEST = 0x0084;
            const int HTCAPTION = 2;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMRIGHT = 16;
            const int HTBOTTOMLEFT = 17;

            switch (msg)
            {
                case WM_SIZING:
                    // Prevent manual resizing
                    handled = true;
                    return IntPtr.Zero;
                    
                case WM_NCHITTEST:
                    var result = DefWindowProc(hwnd, msg, wParam, lParam).ToInt32();
                    // Disable resize handles by converting them to caption
                    if (result >= HTLEFT && result <= HTBOTTOMLEFT && result != HTCAPTION)
                    {
                        handled = true;
                        return new IntPtr(HTCAPTION);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to maximize/restore
                MaximizeButton_Click(sender, e);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                // Single click to drag
                try
                {
                    DragMove();
                }
                catch
                {
                    // Ignore any exceptions from DragMove
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowState = WindowState.Minimized;
                System.Diagnostics.Debug.WriteLine("Window minimized");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error minimizing: {ex.Message}");
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isFullscreen)
                    return;
                
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    System.Diagnostics.Debug.WriteLine("Window restored");
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    System.Diagnostics.Debug.WriteLine("Window maximized");
                }
                
                UpdateMaximizeRestoreButton();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error maximizing/restoring: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Closing window");
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing: {ex.Message}");
            }
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            var fullscreenIcon = FindName("FullscreenIcon") as PackIcon;
            
            if (_isFullscreen)
            {
                _isFullscreen = false;
                WindowStyle = WindowStyle.None; // Keep custom style
                WindowState = _previousWindowState;
                
                if (_previousWindowState == WindowState.Normal)
                {
                    Width = _previousWidth;
                    Height = _previousHeight;
                    Left = _previousLeft;
                    Top = _previousTop;
                }
                
                if (fullscreenIcon != null)
                    fullscreenIcon.Kind = PackIconKind.Fullscreen;
            }
            else
            {
                _isFullscreen = true;
                _previousWindowState = WindowState;
                
                if (WindowState == WindowState.Normal)
                {
                    _previousWidth = Width;
                    _previousHeight = Height;
                    _previousLeft = Left;
                    _previousTop = Top;
                }
                
                WindowState = WindowState.Maximized;
                
                if (fullscreenIcon != null)
                    fullscreenIcon.Kind = PackIconKind.FullscreenExit;
            }
            
            UpdateMaximizeRestoreButton();
        }

        private void UpdateMaximizeRestoreButton()
        {
            var maximizeRestoreIcon = FindName("MaximizeRestoreIcon") as PackIcon;
            var maximizeRestoreButton = FindName("MaximizeRestoreButton") as Button;
            
            if (maximizeRestoreIcon != null && maximizeRestoreButton != null)
            {
                if (WindowState == WindowState.Maximized)
                {
                    maximizeRestoreIcon.Kind = PackIconKind.WindowRestore;
                    maximizeRestoreButton.ToolTip = "Restore";
                }
                else
                {
                    maximizeRestoreIcon.Kind = PackIconKind.WindowMaximize;
                    maximizeRestoreButton.ToolTip = "Maximize";
                }
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SettingsDialog();
                dialog.SetupDialog();
                
                // Just show dialog, settings are saved immediately on toggle
                await DialogService.ShowDialogAsync(dialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing settings: {ex.Message}");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullscreen_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            UpdateMaximizeRestoreButton();
            
            if (!_isFullscreen && (WindowState == WindowState.Normal || WindowState == WindowState.Maximized))
            {
                System.Diagnostics.Debug.WriteLine($"Window state changed to: {WindowState}");
            }
        }
    }
}