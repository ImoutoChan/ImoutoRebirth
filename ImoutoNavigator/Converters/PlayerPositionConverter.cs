using System;
using System.Globalization;
using System.Windows.Data;

namespace Imouto.Navigator.Converters
{
    [ValueConversion(typeof (double), typeof (double))]
    class PlayerPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (float) value;
            if (doubleValue < 0
                || doubleValue > 1)
            {
                return 0;
            }

            return doubleValue * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleValue = (double) value;
            if (doubleValue < 0
                || doubleValue > 100)
            {
                return 0;
            }

            return doubleValue / 100;
        }
    }
}
