using System;
using System.Globalization;
using System.Windows.Data;

namespace AccountManager.Views.Converters
{
    public class MathConverter : IValueConverter
    {
        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo
        }

        public MathOperation Operation { get; set; } = MathOperation.Add;
        public double Operand { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return 0;

                var number = System.Convert.ToDouble(value);
                var operand = Operand;

                // Parse operand from parameter if provided
                if (parameter is string paramString && double.TryParse(paramString, out var paramOperand))
                {
                    operand = paramOperand;
                }

                var result = Operation switch
                {
                    MathOperation.Add => number + operand,
                    MathOperation.Subtract => number - operand,
                    MathOperation.Multiply => number * operand,
                    MathOperation.Divide => operand != 0 ? number / operand : 0,
                    MathOperation.Modulo => operand != 0 ? number % operand : 0,
                    _ => number
                };

                // Convert to target type
                if (targetType == typeof(int))
                    return (int)result;
                else if (targetType == typeof(float))
                    return (float)result;
                else
                    return result;
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return 0;

                var number = System.Convert.ToDouble(value);
                var operand = Operand;

                if (parameter is string paramString && double.TryParse(paramString, out var paramOperand))
                {
                    operand = paramOperand;
                }

                var result = Operation switch
                {
                    MathOperation.Add => number - operand,
                    MathOperation.Subtract => number + operand,
                    MathOperation.Multiply => operand != 0 ? number / operand : 0,
                    MathOperation.Divide => number * operand,
                    MathOperation.Modulo => number, // Can't reverse modulo easily
                    _ => number
                };

                return System.Convert.ChangeType(result, targetType);
            }
            catch
            {
                return 0;
            }
        }
    }
}