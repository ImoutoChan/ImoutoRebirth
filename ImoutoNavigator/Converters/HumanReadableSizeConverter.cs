using System;
using System.Globalization;
using System.Windows.Data;

namespace Imouto.Navigator.Converters
{
    [ValueConversion(typeof (string), typeof (long?))]
    class HumanReadableSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is long))
            {
                return "Unknown";
            }

            var size = (long) value;

            string[] sizes =
            {
                "B",
                "KB",
                "MB",
                "GB"
            };

            var order = 0;

            for (order = 0; size >= 1024 && order + 1 < sizes.Length; order++, size = size / 1024)
            {
                ;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
