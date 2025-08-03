using System.Windows;
using System.Windows.Controls;
using AccountManager.Services;

namespace AccountManager.Views.UserControls
{
    public partial class PasswordStrengthIndicator : UserControl
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordStrengthIndicator),
                new PropertyMetadata(string.Empty, OnPasswordChanged));

        public static readonly DependencyProperty ShowTextProperty =
            DependencyProperty.Register(nameof(ShowText), typeof(bool), typeof(PasswordStrengthIndicator),
                new PropertyMetadata(true));

        public static readonly DependencyProperty StrengthProperty =
            DependencyProperty.Register(nameof(Strength), typeof(int), typeof(PasswordStrengthIndicator),
                new PropertyMetadata(0));

        public static readonly DependencyProperty StrengthTextProperty =
            DependencyProperty.Register(nameof(StrengthText), typeof(string), typeof(PasswordStrengthIndicator),
                new PropertyMetadata("Very Weak"));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public bool ShowText
        {
            get => (bool)GetValue(ShowTextProperty);
            set => SetValue(ShowTextProperty, value);
        }

        public int Strength
        {
            get => (int)GetValue(StrengthProperty);
            private set => SetValue(StrengthProperty, value);
        }

        public string StrengthText
        {
            get => (string)GetValue(StrengthTextProperty);
            private set => SetValue(StrengthTextProperty, value);
        }

        public PasswordStrengthIndicator()
        {
            InitializeComponent();
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordStrengthIndicator indicator)
            {
                indicator.UpdateStrength();
            }
        }

        private void UpdateStrength()
        {
            var strength = ValidationService.Instance.GetPasswordStrength(Password);
            var strengthText = ValidationService.Instance.GetPasswordStrengthDescription(strength);

            Strength = strength;
            StrengthText = strengthText;
        }
    }
}