using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; } = false;
        public bool UseHidden { get; set; } = false;
        public bool TrimWhitespace { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var stringValue = value as string;
                
                if (TrimWhitespace)
                    stringValue = stringValue?.Trim();

                var hasValue = !string.IsNullOrEmpty(stringValue);
                
                if (IsInverted)
                    hasValue = !hasValue;

                return hasValue 
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
            throw new NotImplementedException("StringToVisibilityConverter is one-way only");
        }
    }
}