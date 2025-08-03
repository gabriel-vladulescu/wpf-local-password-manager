using System;
using System.Globalization;
using System.Windows.Data;
using AccountManager.Utilities.Constants;

namespace AccountManager.Views.Converters
{
    public class PasswordDisplayConverter : IMultiValueConverter
    {
        public string MaskedText { get; set; } = AppConstants.UI.DefaultCensoredText;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length == 2 && values[0] is string password && values[1] is bool censorPassword)
                {
                    // For passwords: show dots when censorPassword is TRUE, show actual password when FALSE
                    if (censorPassword)
                    {
                        return string.IsNullOrEmpty(password) ? string.Empty : MaskedText;
                    }
                    return password ?? string.Empty;
                }
                return MaskedText; // Default to censored
            }
            catch
            {
                return MaskedText;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("PasswordDisplayConverter is one-way only");
        }
    }
}