using System;
using System.Globalization;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public string Format { get; set; } = "MM/dd/yyyy HH:mm";
        public string EmptyText { get; set; } = "Never";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is DateTime dateTime)
                {
                    if (dateTime == DateTime.MinValue)
                        return EmptyText;

                    var format = parameter as string ?? Format;
                    return dateTime.ToString(format, culture);
                }
                return EmptyText;
            }
            catch
            {
                return EmptyText;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}