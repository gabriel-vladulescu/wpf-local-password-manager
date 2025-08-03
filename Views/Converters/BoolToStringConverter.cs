using System;
using System.Globalization;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "True";
        public string FalseValue { get; set; } = "False";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    // Check if parameter contains custom values
                    if (parameter is string paramString)
                    {
                        var options = paramString.Split('|');
                        if (options.Length == 2)
                        {
                            return boolValue ? options[0] : options[1];
                        }
                    }
                    
                    return boolValue ? TrueValue : FalseValue;
                }
                return FalseValue;
            }
            catch
            {
                return FalseValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string stringValue)
                {
                    if (parameter is string paramString)
                    {
                        var options = paramString.Split('|');
                        if (options.Length == 2)
                        {
                            return string.Equals(stringValue, options[0], StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    
                    return string.Equals(stringValue, TrueValue, StringComparison.OrdinalIgnoreCase);
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