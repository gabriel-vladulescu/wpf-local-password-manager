using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AccountManager.Views.Converters
{
    public class ValidationErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string error && !string.IsNullOrEmpty(error))
                {
                    if (targetType == typeof(Visibility))
                    {
                        return Visibility.Visible;
                    }
                    else if (targetType == typeof(Brush))
                    {
                        return new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Error red
                    }
                    else if (targetType == typeof(string))
                    {
                        return error;
                    }
                }

                if (targetType == typeof(Visibility))
                {
                    return Visibility.Collapsed;
                }
                else if (targetType == typeof(Brush))
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
                
                return string.Empty;
            }
            catch
            {
                return targetType == typeof(Visibility) ? Visibility.Collapsed : string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ValidationErrorConverter is one-way only");
        }
    }

    /// <summary>
    /// Multi-value converter for conditional validation error display
    /// </summary>
    public class ValidationErrorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length >= 2 && values[0] is string error && values[1] is bool hasError)
                {
                    if (hasError && !string.IsNullOrEmpty(error))
                    {
                        if (targetType == typeof(Visibility))
                            return Visibility.Visible;
                        else if (targetType == typeof(string))
                            return error;
                        else if (targetType == typeof(Brush))
                            return new SolidColorBrush(Color.FromRgb(239, 68, 68));
                    }
                }

                if (targetType == typeof(Visibility))
                    return Visibility.Collapsed;
                else if (targetType == typeof(Brush))
                    return new SolidColorBrush(Colors.Transparent);
                
                return string.Empty;
            }
            catch
            {
                return targetType == typeof(Visibility) ? Visibility.Collapsed : string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ValidationErrorMultiConverter is one-way only");
        }
    }
}