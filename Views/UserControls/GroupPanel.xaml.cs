using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountManager.Models;

namespace AccountManager.Views.UserControls
{
    public partial class GroupPanel : UserControl
    {
        public static readonly DependencyProperty GroupsProperty =
            DependencyProperty.Register(nameof(Groups), typeof(ObservableCollection<AccountGroup>), typeof(GroupPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedGroupProperty =
            DependencyProperty.Register(nameof(SelectedGroup), typeof(AccountGroup), typeof(GroupPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AddGroupCommandProperty =
            DependencyProperty.Register(nameof(AddGroupCommand), typeof(ICommand), typeof(GroupPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditGroupCommandProperty =
            DependencyProperty.Register(nameof(EditGroupCommand), typeof(ICommand), typeof(GroupPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteGroupCommandProperty =
            DependencyProperty.Register(nameof(DeleteGroupCommand), typeof(ICommand), typeof(GroupPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectGroupCommandProperty =
            DependencyProperty.Register(nameof(SelectGroupCommand), typeof(ICommand), typeof(GroupPanel),
                new PropertyMetadata(null));

        public ObservableCollection<AccountGroup> Groups
        {
            get => (ObservableCollection<AccountGroup>)GetValue(GroupsProperty);
            set => SetValue(GroupsProperty, value);
        }

        public AccountGroup SelectedGroup
        {
            get => (AccountGroup)GetValue(SelectedGroupProperty);
            set => SetValue(SelectedGroupProperty, value);
        }

        public ICommand AddGroupCommand
        {
            get => (ICommand)GetValue(AddGroupCommandProperty);
            set => SetValue(AddGroupCommandProperty, value);
        }

        public ICommand EditGroupCommand
        {
            get => (ICommand)GetValue(EditGroupCommandProperty);
            set => SetValue(EditGroupCommandProperty, value);
        }

        public ICommand DeleteGroupCommand
        {
            get => (ICommand)GetValue(DeleteGroupCommandProperty);
            set => SetValue(DeleteGroupCommandProperty, value);
        }

        public ICommand SelectGroupCommand
        {
            get => (ICommand)GetValue(SelectGroupCommandProperty);
            set => SetValue(SelectGroupCommandProperty, value);
        }

        public GroupPanel()
        {
            InitializeComponent();
        }

        private void OnGroupDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is AccountGroup group)
            {
                if (EditGroupCommand?.CanExecute(group) == true)
                {
                    EditGroupCommand.Execute(group);
                    e.Handled = true;
                }
            }
        }
    }
}