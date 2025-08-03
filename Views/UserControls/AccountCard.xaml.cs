using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Models;

namespace AccountManager.Views.UserControls
{
    public partial class AccountCard : UserControl
    {
        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(nameof(Account), typeof(Account), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CopyEmailCommandProperty =
            DependencyProperty.Register(nameof(CopyEmailCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CopyUsernameCommandProperty =
            DependencyProperty.Register(nameof(CopyUsernameCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CopyPasswordCommandProperty =
            DependencyProperty.Register(nameof(CopyPasswordCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CopyWebsiteCommandProperty =
            DependencyProperty.Register(nameof(CopyWebsiteCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty OpenWebsiteCommandProperty =
            DependencyProperty.Register(nameof(OpenWebsiteCommand), typeof(ICommand), typeof(AccountCard),
                new PropertyMetadata(null));

        public Account Account
        {
            get => (Account)GetValue(AccountProperty);
            set => SetValue(AccountProperty, value);
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public ICommand CopyEmailCommand
        {
            get => (ICommand)GetValue(CopyEmailCommandProperty);
            set => SetValue(CopyEmailCommandProperty, value);
        }

        public ICommand CopyUsernameCommand
        {
            get => (ICommand)GetValue(CopyUsernameCommandProperty);
            set => SetValue(CopyUsernameCommandProperty, value);
        }

        public ICommand CopyPasswordCommand
        {
            get => (ICommand)GetValue(CopyPasswordCommandProperty);
            set => SetValue(CopyPasswordCommandProperty, value);
        }

        public ICommand CopyWebsiteCommand
        {
            get => (ICommand)GetValue(CopyWebsiteCommandProperty);
            set => SetValue(CopyWebsiteCommandProperty, value);
        }

        public ICommand OpenWebsiteCommand
        {
            get => (ICommand)GetValue(OpenWebsiteCommandProperty);
            set => SetValue(OpenWebsiteCommandProperty, value);
        }

        public AccountCard()
        {
            InitializeComponent();
        }

        private void OnCardDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EditCommand?.CanExecute(Account) == true)
            {
                EditCommand.Execute(Account);
                e.Handled = true;
            }
        }
    }
}