using System.Windows;
using System.Windows.Controls;

namespace AccountManager.Utilities.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject obj) => 
            (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value) => 
            obj.SetValue(PlaceholderProperty, value);

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached(
                "Label",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(string.Empty));

        public static string GetLabel(DependencyObject obj) =>
            (string)obj.GetValue(LabelProperty);

        public static void SetLabel(DependencyObject obj, string value) =>
            obj.SetValue(LabelProperty, value);

        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.RegisterAttached(
                "Info",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(string.Empty));

        public static string GetInfo(DependencyObject obj) =>
            (string)obj.GetValue(InfoProperty);

        public static void SetInfo(DependencyObject obj, string value) =>
            obj.SetValue(InfoProperty, value);

        public static readonly DependencyProperty MaxCharsProperty =
            DependencyProperty.RegisterAttached(
                "MaxChars",
                typeof(int),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(0));

        public static int GetMaxChars(DependencyObject obj) => 
            (int)obj.GetValue(MaxCharsProperty);

        public static void SetMaxChars(DependencyObject obj, int value) => 
            obj.SetValue(MaxCharsProperty, value);
    }
}
