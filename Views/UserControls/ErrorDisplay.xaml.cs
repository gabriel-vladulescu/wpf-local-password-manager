using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccountManager.Views.UserControls
{
    public partial class ErrorDisplay : UserControl
    {
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(ErrorDisplay),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(ErrorDisplay),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DismissCommandProperty =
            DependencyProperty.Register(nameof(DismissCommand), typeof(ICommand), typeof(ErrorDisplay),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ShowDetailsProperty =
            DependencyProperty.Register(nameof(ShowDetails), typeof(bool), typeof(ErrorDisplay),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ErrorDetailsProperty =
            DependencyProperty.Register(nameof(ErrorDetails), typeof(string), typeof(ErrorDisplay),
                new PropertyMetadata(string.Empty));

        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public ICommand DismissCommand
        {
            get => (ICommand)GetValue(DismissCommandProperty);
            set => SetValue(DismissCommandProperty, value);
        }

        public bool ShowDetails
        {
            get => (bool)GetValue(ShowDetailsProperty);
            set => SetValue(ShowDetailsProperty, value);
        }

        public string ErrorDetails
        {
            get => (string)GetValue(ErrorDetailsProperty);
            set => SetValue(ErrorDetailsProperty, value);
        }

        public ErrorDisplay()
        {
            InitializeComponent();
        }

        private void OnDismissClick(object sender, RoutedEventArgs e)
        {
            if (DismissCommand?.CanExecute(null) == true)
            {
                DismissCommand.Execute(null);
            }
        }

        private void OnToggleDetailsClick(object sender, RoutedEventArgs e)
        {
            ShowDetails = !ShowDetails;
        }
    }
}