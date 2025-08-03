using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AccountManager.Services;

namespace AccountManager.Views.Converters
{
    public class PasswordStrengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string password)
                {
                    var strength = ValidationService.Instance.GetPasswordStrength(password);
                    
                    if (targetType == typeof(string))
                    {
                        return ValidationService.Instance.GetPasswordStrengthDescription(strength);
                    }
                    else if (targetType == typeof(Brush))
                    {
                        return strength switch
                        {
                            0 => new SolidColorBrush(Color.FromRgb(156, 163, 175)), // Gray
                            1 => new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // Red
                            2 => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  // Orange
                            3 => new SolidColorBrush(Color.FromRgb(59, 130, 246)),  // Blue
                            4 => new SolidColorBrush(Color.FromRgb(16, 185, 129)),  // Green
                            _ => new SolidColorBrush(Color.FromRgb(156, 163, 175))
                        };
                    }
                    else if (targetType == typeof(double))
                    {
                        return (double)strength / 4.0 * 100.0; // Percentage
                    }
                }
                
                return targetType == typeof(string) ? "Unknown" : 
                       targetType == typeof(Brush) ? new SolidColorBrush(Colors.Gray) : 0.0;
            }
            catch
            {
                return targetType == typeof(string) ? "Unknown" : 
                       targetType == typeof(Brush) ? new SolidColorBrush(Colors.Gray) : 0.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("PasswordStrengthConverter is one-way only");
        }
    }
}