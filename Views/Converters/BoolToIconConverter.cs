using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace AccountManager.Views.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public PackIconKind TrueIcon { get; set; } = PackIconKind.Check;
        public PackIconKind FalseIcon { get; set; } = PackIconKind.Close;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool boolValue)
                {
                    if (parameter is string paramString)
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
                    
                    return boolValue ? TrueIcon : FalseIcon;
                }
                return FalseIcon;
            }
            catch
            {
                return PackIconKind.Help;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BoolToIconConverter is one-way only");
        }
    }
}