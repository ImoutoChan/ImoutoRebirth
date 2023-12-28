using System.Globalization;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Converters;

[ValueConversion(typeof(double), typeof(double))]
internal class PlayerPositionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
            return 0;

        if (doubleValue is < 0 or > 1)
            return 0;

        return doubleValue * 100;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
            return 0;

        if (doubleValue is < 0 or > 100)
            return 0;

        return doubleValue / 100;
    }
}
