using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.Converters;

[ValueConversion(typeof(Color), typeof(int))]
internal class IntToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue)
            throw new ArgumentException("The input variable has wrong type.");

        return new SolidColorBrush(intValue.ToColor());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
