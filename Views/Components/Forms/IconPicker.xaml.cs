using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Views.Components
{
    public partial class IconPicker : UserControl
    {
        public static readonly DependencyProperty SelectedIconProperty =
            DependencyProperty.Register(
                nameof(SelectedIcon),
                typeof(PackIconKind),
                typeof(IconPicker),
                new FrameworkPropertyMetadata(
                    PackIconKind.Folder, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                ));

        public PackIconKind SelectedIcon
        {
            get => (PackIconKind)GetValue(SelectedIconProperty);
            set => SetValue(SelectedIconProperty, value);
        }

        public static readonly DependencyProperty IconsProperty =
            DependencyProperty.Register(
                nameof(Icons),
                typeof(List<IconItem>),
                typeof(IconPicker),
                new PropertyMetadata(default(List<IconItem>)));

        public List<IconItem> Icons
        {
            get => (List<IconItem>)GetValue(IconsProperty);
            set => SetValue(IconsProperty, value);
        }

        public IconPicker()
        {
            InitializeComponent();
            
            if (DesignerProperties.GetIsInDesignMode(this)) {
                DataContext = this;
            }

            // Initialize icons collection
            Icons = new List<IconItem>
            {
                // Row 1
                new IconItem { Value = "Folder", Kind = PackIconKind.Folder },
                new IconItem { Value = "Briefcase", Kind = PackIconKind.Briefcase },
                new IconItem { Value = "ShoppingCart", Kind = PackIconKind.ShoppingCart },
                new IconItem { Value = "Home", Kind = PackIconKind.Home },
                new IconItem { Value = "Heart", Kind = PackIconKind.Heart },
                new IconItem { Value = "Clock", Kind = PackIconKind.Clock },
                new IconItem { Value = "DotsGrid", Kind = PackIconKind.DotsGrid },
                new IconItem { Value = "Cog", Kind = PackIconKind.Cog },
                new IconItem { Value = "Account", Kind = PackIconKind.Account },
                new IconItem { Value = "Star", Kind = PackIconKind.Star },
                
                // Row 2
                new IconItem { Value = "Leaf", Kind = PackIconKind.Leaf },
                new IconItem { Value = "Shield", Kind = PackIconKind.Shield },
                new IconItem { Value = "Basketball", Kind = PackIconKind.Basketball },
                new IconItem { Value = "CreditCard", Kind = PackIconKind.CreditCard },
                new IconItem { Value = "Phone", Kind = PackIconKind.Phone },
                new IconItem { Value = "Smiley", Kind = PackIconKind.Smiley },
                new IconItem { Value = "Lock", Kind = PackIconKind.Lock },
                new IconItem { Value = "AccountGroup", Kind = PackIconKind.AccountGroup },
                new IconItem { Value = "Target", Kind = PackIconKind.Target },
                new IconItem { Value = "Refresh", Kind = PackIconKind.Refresh },
                
                // Row 3
                new IconItem { Value = "Fire", Kind = PackIconKind.Fire },
                new IconItem { Value = "Calendar", Kind = PackIconKind.Calendar },
                new IconItem { Value = "Diamond", Kind = PackIconKind.Diamond },
                new IconItem { Value = "Cloud", Kind = PackIconKind.Cloud },
                new IconItem { Value = "Monitor", Kind = PackIconKind.Monitor },
                new IconItem { Value = "Circle", Kind = PackIconKind.Circle },
                new IconItem { Value = "Book", Kind = PackIconKind.Book },
                new IconItem { Value = "Map", Kind = PackIconKind.Map },
                new IconItem { Value = "Cube", Kind = PackIconKind.Cube },
                new IconItem { Value = "Cancel", Kind = PackIconKind.Cancel },
                
                // Row 4
                new IconItem { Value = "FileDocument", Kind = PackIconKind.FileDocument },
                new IconItem { Value = "Tag", Kind = PackIconKind.Tag },
                new IconItem { Value = "LocationEnter", Kind = PackIconKind.LocationEnter },
                new IconItem { Value = "Laptop", Kind = PackIconKind.Laptop },
                new IconItem { Value = "Bullseye", Kind = PackIconKind.Bullseye },
                new IconItem { Value = "BookOpen", Kind = PackIconKind.BookOpen },
                new IconItem { Value = "FolderMultiple", Kind = PackIconKind.FolderMultiple },
                new IconItem { Value = "Gamepad", Kind = PackIconKind.Gamepad },
                new IconItem { Value = "ChartLine", Kind = PackIconKind.ChartLine },
                new IconItem { Value = "FormatListBulleted", Kind = PackIconKind.FormatListBulleted }
            };
        }
    }

    public class IconItem
    {
        public string Value { get; set; }
        public PackIconKind Kind { get; set; }
    }
}