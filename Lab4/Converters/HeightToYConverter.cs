using System;
using System.Globalization;
using System.Windows.Data;

namespace Lab4.Converters
{
    public class HeightToYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height)
            {
                // Position from bottom by negating the height
                return -height;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
