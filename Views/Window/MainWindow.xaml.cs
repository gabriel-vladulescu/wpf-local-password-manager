using System;
using System.Windows;
using System.Windows.Interop;
using AccountManager.ViewModels;

namespace AccountManager.Views.Window
{
    public partial class MainWindow : System.Windows.Window
    {
        private double _normalWidth;
        private double _normalHeight;
        private double _normalLeft;
        private double _normalTop;
        private bool _isMaximizedToWorkingArea = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Store initial window dimensions and position
            _normalWidth = this.Width;
            _normalHeight = this.Height;
            _normalLeft = this.Left;
            _normalTop = this.Top;
            
            DialogService.Initialize(this);
        }

        public void ToggleMaximizeRestore()
        {
            if (_isMaximizedToWorkingArea)
                RestoreWindow();
            else
                MaximizeToWorkingArea();
        }

        public bool IsMaximizedToWorkingArea => _isMaximizedToWorkingArea;

        private void MaximizeToWorkingArea()
        {
            // Store current position/size before maximizing
            if (!_isMaximizedToWorkingArea)
            {
                _normalLeft = this.Left;
                _normalTop = this.Top;
                _normalWidth = this.Width;
                _normalHeight = this.Height;
            }

            var workingArea = SystemParameters.WorkArea;
            this.Left = workingArea.Left;
            this.Top = workingArea.Top;
            this.Width = workingArea.Width;
            this.Height = workingArea.Height;
            
            _isMaximizedToWorkingArea = true;
        }

        private void RestoreWindow()
        {
            this.Left = _normalLeft;
            this.Top = _normalTop;
            this.Width = _normalWidth;
            this.Height = _normalHeight;
            
            _isMaximizedToWorkingArea = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SIZING = 0x0214;
            const int WM_NCHITTEST = 0x0084;
            const int HTCAPTION = 2;
            const int HTLEFT = 10;
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
    }
}