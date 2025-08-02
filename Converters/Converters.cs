using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue && parameter is string paramString)
                {
                    var options = paramString.Split('|');
                    if (options.Length == 2)
                    {
                        return boolValue ? options[0] : options[1];
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue && parameter is string paramString)
                {
                    var options = paramString.Split('|');
                    if (options.Length == 2)
                    {
                        var iconName = boolValue ? options[0] : options[1];
                        if (Enum.TryParse<PackIconKind>(iconName, out var iconKind))
                        {
                            return iconKind;
                        }
                    }
                }
                return PackIconKind.Account;
            }
            catch
            {
                return PackIconKind.Account;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string text && !string.IsNullOrEmpty(text))
                {
                    return text.Substring(0, 1).ToUpper();
                }
                return "A";
            }
            catch
            {
                return "A";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}