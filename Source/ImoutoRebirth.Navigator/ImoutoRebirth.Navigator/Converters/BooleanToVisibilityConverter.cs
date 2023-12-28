using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Converters;

[ValueConversion(typeof(Visibility), typeof(bool))]
internal class BooleanToVisibilityConverter : IValueConverter
{
    public bool IsInvert { private get; set; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) 
            throw new ArgumentException("The input variable has wrong type.");

        if (IsInvert)
            boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Visibility visibility)
            throw new ArgumentException("The input variable has wrong type.");

        var result = visibility == Visibility.Visible;
        return IsInvert ? !result : result;
    }
}
