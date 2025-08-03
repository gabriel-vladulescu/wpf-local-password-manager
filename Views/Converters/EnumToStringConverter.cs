using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public bool UseDescriptionAttribute { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is Enum enumValue)
                {
                    if (UseDescriptionAttribute)
                    {
                        var field = enumValue.GetType().GetField(enumValue.ToString());
                        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
                        if (attribute != null)
                        {
                            return attribute.Description;
                        }
                    }
                    
                    return enumValue.ToString();
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
            try
            {
                if (value is string stringValue && targetType.IsEnum)
                {
                    return Enum.Parse(targetType, stringValue);
                }
                return Activator.CreateInstance(targetType);
            }
            catch
            {
                return Activator.CreateInstance(targetType);
            }
        }
    }
}