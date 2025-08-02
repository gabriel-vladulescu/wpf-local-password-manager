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
                    handled = true;
                    return IntPtr.Zero;
                    
                case WM_NCHITTEST:
                    var result = DefWindowProc(hwnd, msg, wParam, lParam).ToInt32();
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

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullscreen)
                return;
            
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            var fullscreenIcon = FindName("FullscreenIcon") as PackIcon;
            
            if (_isFullscreen)
            {
                _isFullscreen = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
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
                
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                
                if (fullscreenIcon != null)
                    fullscreenIcon.Kind = PackIconKind.FullscreenExit;
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
            
            if (!_isFullscreen && (WindowState == WindowState.Normal || WindowState == WindowState.Maximized))
            {
                System.Diagnostics.Debug.WriteLine($"Window state changed to: {WindowState}");
            }
        }
    }
}