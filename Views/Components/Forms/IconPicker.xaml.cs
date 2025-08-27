using System;
using System.Linq;
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

        private Random _random = new Random();

        public IconPicker()
        {

            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = this;
            }

            GenerateRandomIcons();
            DataContext = this;
        }

        private void GenerateRandomIcons()
        {
            Icons = Enum.GetValues(typeof(PackIconKind))
                .Cast<PackIconKind>()
                .Where(k => k.ToString().EndsWith("Outline", StringComparison.Ordinal))
                .OrderBy(x => _random.Next())   // shuffle
                .Take(40)
                .Select(k => new IconItem
                {
                    // Value = k.ToString().Replace("Outline", ""), // optional: cleaner name
                    Value = k.ToString(),
                    Kind = k
                })
                .ToList();
        }

        private void RefreshIcons_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomIcons();
            SetValue(IconsProperty, Icons);
        }
    }

    public class IconItem
    {
        public string Value { get; set; }
        public PackIconKind Kind { get; set; }
    }
}