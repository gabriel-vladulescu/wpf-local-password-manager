using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; } = false;
        public bool UseHidden { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    if (IsInverted)
                        boolValue = !boolValue;

                    return boolValue 
                        ? Visibility.Visible 
                        : (UseHidden ? Visibility.Hidden : Visibility.Collapsed);
                }

                return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
            }
            catch
            {
                return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Visibility visibility)
                {
                    var result = visibility == Visibility.Visible;
                    return IsInverted ? !result : result;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}