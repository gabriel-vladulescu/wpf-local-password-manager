using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AccountManager.Views.Components
{
    public partial class ToastNotification : UserControl
    {
        private DispatcherTimer _autoCloseTimer;
        private bool _isClosing;
        private bool _autoCloseEnabled = true;
        
        public event EventHandler Closed;

        public enum ToastType
        {
            Info,
            Success,
            Warning,
            Error
        }

        public ToastNotification()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Start slide-in animation
            var slideIn = (Storyboard)Resources["SlideInAnimation"];
            slideIn.Begin();
            
            // Start progress bar animation if auto-close is enabled
            if (_autoCloseEnabled)
            {
                var progressAnimation = (Storyboard)Resources["ProgressBarAnimation"];
                progressAnimation.Begin();
                
                // Auto-close timer
                _autoCloseTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(4)
                };
                _autoCloseTimer.Tick += (s, args) => Close();
                _autoCloseTimer.Start();
            }
        }

        public void SetContent(string title, string message, ToastType type, bool autoClose = true)
        {
            ToastTitle.Text = title;
            ToastMessage.Text = message;
            _autoCloseEnabled = autoClose;
            
            // Set icon and colors based on type
            switch (type)
            {
                case ToastType.Info:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                    ToastIcon.SetResourceReference(Control.ForegroundProperty, "PrimaryColor");
                    ProgressBar.SetResourceReference(Border.BackgroundProperty, "PrimaryColor");
                    break;
                case ToastType.Success:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                    ToastIcon.SetResourceReference(Control.ForegroundProperty, "SuccessColor");
                    ProgressBar.SetResourceReference(Border.BackgroundProperty, "SuccessColor");
                    break;
                case ToastType.Warning:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircle;
                    ToastIcon.SetResourceReference(Control.ForegroundProperty, "WarningColor");
                    ProgressBar.SetResourceReference(Border.BackgroundProperty, "WarningColor");
                    break;
                case ToastType.Error:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle;
                    ToastIcon.SetResourceReference(Control.ForegroundProperty, "DangerColor");
                    ProgressBar.SetResourceReference(Border.BackgroundProperty, "DangerColor");
                    // Errors don't auto-close by default
                    _autoCloseEnabled = false;
                    break;
            }
            
            // Hide progress bar if auto-close is disabled
            if (!_autoCloseEnabled)
            {
                ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent event bubbling
            Close();
        }

        private void ToastCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Only close on card click, not close button click
            if (e.OriginalSource != CloseButton && !IsDescendantOf(e.OriginalSource as DependencyObject, CloseButton))
            {
                Close();
            }
        }

        private bool IsDescendantOf(DependencyObject child, DependencyObject parent)
        {
            if (child == null || parent == null) return false;
            if (child == parent) return true;
            
            var parentOfChild = System.Windows.Media.VisualTreeHelper.GetParent(child);
            return IsDescendantOf(parentOfChild, parent);
        }

        private void Close()
        {
            if (_isClosing) return;
            _isClosing = true;
            
            _autoCloseTimer?.Stop();
            
            // Stop the progress bar animation immediately
            var progressAnimation = (Storyboard)Resources["ProgressBarAnimation"];
            progressAnimation.Stop();
            
            // Reset progress bar to 0 when closing
            if (ProgressBarTransform != null)
            {
                ProgressBarTransform.ScaleX = 0;
            }
            
            var slideOut = (Storyboard)Resources["SlideOutAnimation"];
            slideOut.Completed += (s, e) => 
            {
                Closed?.Invoke(this, EventArgs.Empty);
            };
            slideOut.Begin();
        }
    }
}