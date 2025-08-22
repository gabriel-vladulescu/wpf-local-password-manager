using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Utilities.Converters
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
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string stringValue = value.ToString();
            string targetValue = parameter.ToString();

            return string.Equals(stringValue, targetValue, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
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

    public class StringEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return parameter.ToString();
            return Binding.DoNothing;
        }
    }

    public class MultiStringEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] != null && values[1] != null)
                return values[0].ToString() == values[1].ToString();

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class OpacityBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double opacity = 0.1;

            if (parameter != null)
            {
                var paramString = parameter.ToString().Replace(',', '.');
                if (!double.TryParse(paramString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double paramOpacity))
                {
                    throw new ArgumentException($"ConverterParameter '{parameter}' is not a valid double.");
                }
                opacity = paramOpacity;
            }

            Color color;

            if (value is string colorString)
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(colorString);
                }
                catch
                {
                  //  throw new ArgumentException($"Failed to parse color string: {colorString}");
                }
            }
            else if (value is SolidColorBrush brush)
            {
                color = brush.Color;
            }
            else if (value is Color c)
            {
                color = c;
            }
            else
            {
                throw new ArgumentException($"Value is neither string, SolidColorBrush nor Color, returning original value.");
            }

            byte alpha = (byte)(opacity * 255);
            Color colorWithAlpha = Color.FromArgb(alpha, color.R, color.G, color.B);

            return new SolidColorBrush(colorWithAlpha);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return false;

            return object.Equals(values[0], values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CharacterLimitConverter : IMultiValueConverter
    {
        public static readonly CharacterLimitConverter Instance = new CharacterLimitConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return false;

            if (values[0] is int length && TryParseMax(values[1], out int max))
            {
                return max > 0 && length >= (int)(max * 0.8) && length <= max;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private bool TryParseMax(object value, out int result)
        {
            if (value is int i)
            {
                result = i;
                return true;
            }

            if (value is string s && int.TryParse(s, out int parsed))
            {
                result = parsed;
                return true;
            }

            result = 0;
            return false;
        }
    }

    public class CharacterLimitExceededConverter : IMultiValueConverter
    {
        public static readonly CharacterLimitExceededConverter Instance = new CharacterLimitExceededConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return false;

            if (values[0] is int length && TryParseMax(values[1], out int max))
            {
                return length > max;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private bool TryParseMax(object value, out int result)
        {
            if (value is int i)
            {
                result = i;
                return true;
            }

            if (value is string s && int.TryParse(s, out int parsed))
            {
                result = parsed;
                return true;
            }

            result = 0;
            return false;
        }
    }

}