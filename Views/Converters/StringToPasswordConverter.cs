using System;
using System.Globalization;
using System.Windows.Data;
using AccountManager.Utilities.Constants;

namespace AccountManager.Views.Converters
{
    public class StringToPasswordConverter : IValueConverter
    {
        public string MaskCharacter { get; set; } = "•";
        public int MaskLength { get; set; } = 12;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string password && !string.IsNullOrEmpty(password))
                {
                    return new string(MaskCharacter[0], MaskLength);
                }
                return string.Empty;
            }
            catch
            {
                return AppConstants.UI.DefaultCensoredText;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("StringToPasswordConverter is one-way only");
        }
    }
}