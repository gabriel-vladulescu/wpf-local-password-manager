using System;
using System.Globalization;
using System.Windows.Data;
using AccountManager.Utilities.Constants;

namespace AccountManager.Views.Converters
{
    public class ConditionalTextConverter : IMultiValueConverter
    {
        public string CensoredText { get; set; } = AppConstants.UI.DefaultCensoredText;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length == 2 && values[0] is string text && values[1] is bool shouldCensor)
                {
                    if (shouldCensor && !string.IsNullOrEmpty(text))
                    {
                        return CensoredText;
                    }
                    return text ?? string.Empty;
                }
                return values[0]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConditionalTextConverter is one-way only");
        }
    }
}