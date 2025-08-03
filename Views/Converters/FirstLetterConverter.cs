using System;
using System.Globalization;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public bool ToUpperCase { get; set; } = true;
        public string DefaultValue { get; set; } = "A";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string text && !string.IsNullOrEmpty(text))
                {
                    var firstChar = text.Substring(0, 1);
                    return ToUpperCase ? firstChar.ToUpper() : firstChar;
                }
                return DefaultValue;
            }
            catch
            {
                return DefaultValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("FirstLetterConverter is one-way only");
        }
    }
}