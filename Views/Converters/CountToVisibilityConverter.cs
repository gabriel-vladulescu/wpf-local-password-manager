using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; } = false;
        public bool UseHidden { get; set; } = false;
        public int Threshold { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int count = 0;
                
                if (value is IEnumerable enumerable)
                {
                    count = enumerable.Cast<object>().Count();
                }
                else if (value is int intValue)
                {
                    count = intValue;
                }

                var hasItems = count > Threshold;
                
                if (IsInverted)
                    hasItems = !hasItems;

                return hasItems 
                    ? Visibility.Visible 
                    : (UseHidden ? Visibility.Hidden : Visibility.Collapsed);
            }
            catch
            {
                return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("CountToVisibilityConverter is one-way only");
        }
    }
}