using System;
using System.Globalization;
using System.Windows;
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

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CensoredTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string text && !string.IsNullOrEmpty(text))
                {
                    return "••••••••••••";
                }
                return value;
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConditionalTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length == 2 && values[0] is string text && values[1] is bool shouldCensor)
                {
                    if (shouldCensor && !string.IsNullOrEmpty(text))
                    {
                        return "••••••••••••";
                    }
                    return text;
                }
                return values[0];
            }
            catch
            {
                return values[0];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PasswordDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length == 2 && values[0] is string password && values[1] is bool censorPassword)
                {
                    // For passwords: show dots when censorPassword is TRUE, show actual password when FALSE
                    if (censorPassword)
                    {
                        return "••••••••••••";
                    }
                    return string.IsNullOrEmpty(password) ? "" : password;
                }
                return "••••••••••••"; // Default to censored
            }
            catch
            {
                return "••••••••••••";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}