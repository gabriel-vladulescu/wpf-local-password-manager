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
            
            // Auto-close timer
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            _autoCloseTimer.Tick += (s, args) => Close();
            _autoCloseTimer.Start();
        }

        public void SetContent(string title, string message, ToastType type)
        {
            ToastTitle.Text = title;
            ToastMessage.Text = message;
            
            // Set icon and colors based on type
            switch (type)
            {
                case ToastType.Info:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Information;
                    ToastIcon.SetResourceReference(ForegroundProperty, "PrimaryColor");
                    break;
                case ToastType.Success:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                    ToastIcon.SetResourceReference(ForegroundProperty, "SuccessColor");
                    break;
                case ToastType.Warning:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircle;
                    ToastIcon.SetResourceReference(ForegroundProperty, "WarningColor");
                    break;
                case ToastType.Error:
                    ToastIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle;
                    ToastIcon.SetResourceReference(ForegroundProperty, "DangerColor");
                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            _autoCloseTimer?.Stop();
            
            var slideOut = (Storyboard)Resources["SlideOutAnimation"];
            slideOut.Completed += (s, e) => 
            {
                Closed?.Invoke(this, EventArgs.Empty);
            };
            slideOut.Begin();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Close on click
            Close();
        }
    }
}