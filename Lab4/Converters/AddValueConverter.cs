using System;
using System.Globalization;
using System.Windows.Data;

namespace Lab4.Converters
{
    public class AddValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string stringParameter)
            {
                if (double.TryParse(stringParameter, out double parameterValue))
                {
                    return doubleValue + parameterValue;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
