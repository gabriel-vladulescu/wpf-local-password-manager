using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; } = false;
        public bool UseHidden { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var isNull = value == null;
                
                if (IsInverted)
                    isNull = !isNull;

                return isNull 
                    ? (UseHidden ? Visibility.Hidden : Visibility.Collapsed)
                    : Visibility.Visible;
            }
            catch
            {
                return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("NullToVisibilityConverter is one-way only");
        }
    }
}