using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AccountManager.Views.Components
{
    public partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                nameof(SelectedColor),
                typeof(string),
                typeof(ColorPicker),
                new FrameworkPropertyMetadata(
                    "#a779ff", 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                ));

        public string SelectedColor
        {
            get => (string)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register(
                nameof(Colors),
                typeof(List<ColorItem>),
                typeof(ColorPicker),
                new PropertyMetadata(default(List<ColorItem>)));

        public List<ColorItem> Colors
        {
            get => (List<ColorItem>)GetValue(ColorsProperty);
            set => SetValue(ColorsProperty, value);
        }

        public ColorPicker()
        {
            InitializeComponent();
            
            if (DesignerProperties.GetIsInDesignMode(this)) {
                DataContext = this;
            }

            // Initialize colors collection
            Colors = new List<ColorItem>
            {
                new ColorItem { Value = "#a779ff", Name = "Purple" },
                new ColorItem { Value = "#f39293", Name = "Pink" },
                new ColorItem { Value = "#f7d775", Name = "Yellow" },
                new ColorItem { Value = "#90c699", Name = "Green" },
                new ColorItem { Value = "#92b2f3", Name = "Blue" },
                new ColorItem { Value = "#eb8cd7", Name = "Magenta" },
                new ColorItem { Value = "#cd5a6f", Name = "Red" },
                new ColorItem { Value = "#e5a366", Name = "Orange" },
                new ColorItem { Value = "#e6e7e7", Name = "Gray" },
                new ColorItem { Value = "#9fe2e7", Name = "Cyan" }
            };
        }
    }

    public class ColorItem
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }
}