using System.Windows;
using System.Windows.Controls;
using System.Collections;
using AccountManager.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Views.Window.Navigation
{
    public partial class SystemGroupsList : UserControl
    {
        public static readonly DependencyProperty GroupTypeProperty =
            DependencyProperty.Register(nameof(GroupType), typeof(GroupDisplayType), typeof(SystemGroupsList), 
                new PropertyMetadata(GroupDisplayType.TopSystem, OnGroupTypeChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SystemGroupsList), 
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SystemGroupsList), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public SystemGroupsList()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public GroupDisplayType GroupType
        {
            get => (GroupDisplayType)GetValue(GroupTypeProperty);
            set => SetValue(GroupTypeProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupBindings();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && DataContext is SidebarViewModel sidebarViewModel)
            {
                // Get the group item from the button's DataContext
                var groupItem = button.DataContext;
                
                // Create the context menu programmatically
                var contextMenu = new ContextMenu
                {
                    Background = (System.Windows.Media.Brush)FindResource("ContextMenuBackgroundColor"),
                    BorderBrush = (System.Windows.Media.Brush)FindResource("ContextMenuBorderColor"),
                    BorderThickness = new Thickness(1)
                };

                // Edit menu item
                var editMenuItem = new MenuItem
                {
                    Header = "Edit",
                    Command = sidebarViewModel.EditGroupCommand,
                    CommandParameter = groupItem,
                    Style = (Style)FindResource(typeof(MenuItem))
                };
                
                // Edit icon
                var editIcon = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.PencilOutline,
                    Foreground = (System.Windows.Media.Brush)FindResource("SidebarTextPrimaryColor"),
                    Width = 16,
                    Height = 16
                };
                editMenuItem.Icon = editIcon;

                // Delete menu item
                var deleteMenuItem = new MenuItem
                {
                    Header = "Delete",
                    Command = sidebarViewModel.DeleteGroupCommand,
                    CommandParameter = groupItem,
                    Style = (Style)FindResource(typeof(MenuItem))
                };
                
                // Delete icon
                var deleteIcon = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.DeleteOutline,
                    Foreground = (System.Windows.Media.Brush)FindResource("SidebarTextPrimaryColor"),
                    Width = 16,
                    Height = 16
                };
                deleteMenuItem.Icon = deleteIcon;

                // Add items to context menu
                contextMenu.Items.Add(editMenuItem);
                contextMenu.Items.Add(deleteMenuItem);

                // Show the context menu
                contextMenu.PlacementTarget = button;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }
        }

        public void SetGroupType(GroupDisplayType groupType)
        {
            GroupType = groupType;
            if (IsLoaded)
            {
                SetupBindings();
            }
        }

        private static void OnGroupTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SystemGroupsList control)
            {
                control.SetupBindings();
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SystemGroupsList control && control.IsLoaded && control.GroupsListBox != null)
            {
                control.GroupsListBox.ItemsSource = (IEnumerable)e.NewValue;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SystemGroupsList control && control.IsLoaded && control.GroupsListBox != null)
            {
                control.GroupsListBox.SelectedItem = e.NewValue;
            }
        }

        private void SetupBindings()
        {
            if (DataContext is SidebarViewModel sidebarViewModel && GroupsListBox != null)
            {
                // Set up ItemsSource and SelectedItem based on GroupType
                switch (GroupType)
                {
                    case GroupDisplayType.TopSystem:
                        GroupsListBox.SetBinding(ListBox.ItemsSourceProperty, 
                            new System.Windows.Data.Binding("TopSystemGroups") { Source = sidebarViewModel });
                        GroupsListBox.SetBinding(ListBox.SelectedItemProperty, 
                            new System.Windows.Data.Binding("SelectedSystemGroup") { Source = sidebarViewModel, Mode = System.Windows.Data.BindingMode.TwoWay });
                        break;

                    case GroupDisplayType.Regular:
                        GroupsListBox.SetBinding(ListBox.ItemsSourceProperty, 
                            new System.Windows.Data.Binding("RegularGroups") { Source = sidebarViewModel });
                        GroupsListBox.SetBinding(ListBox.SelectedItemProperty, 
                            new System.Windows.Data.Binding("SelectedGroup") { Source = sidebarViewModel, Mode = System.Windows.Data.BindingMode.TwoWay });
                        break;

                    case GroupDisplayType.BottomSystem:
                        GroupsListBox.SetBinding(ListBox.ItemsSourceProperty, 
                            new System.Windows.Data.Binding("BottomSystemGroups") { Source = sidebarViewModel });
                        GroupsListBox.SetBinding(ListBox.SelectedItemProperty, 
                            new System.Windows.Data.Binding("SelectedSystemGroup") { Source = sidebarViewModel, Mode = System.Windows.Data.BindingMode.TwoWay });
                        break;
                }

                // Set up click command binding for system groups
                if (GroupType == GroupDisplayType.TopSystem || GroupType == GroupDisplayType.BottomSystem)
                {
                    // For system groups, we need to handle clicks differently
                    GroupsListBox.SelectionChanged += (s, e) =>
                    {
                        if (e.AddedItems.Count > 0 && sidebarViewModel.SelectSystemGroupCommand.CanExecute(e.AddedItems[0]))
                        {
                            sidebarViewModel.SelectSystemGroupCommand.Execute(e.AddedItems[0]);
                        }
                    };
                }
            }
        }
    }

    public enum GroupDisplayType
    {
        TopSystem,
        Regular,
        BottomSystem
    }
}