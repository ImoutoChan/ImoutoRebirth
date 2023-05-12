using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Converters;

[ValueConversion(typeof(Visibility), typeof(Boolean))]
class BooleanToVisibilityConverter : IValueConverter
{
    public BooleanToVisibilityConverter()
    {
        IsInvert = false;
    }

    public bool IsInvert { private get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is Boolean)) throw new ArgumentException("The input variable has wrong type.");

        var isCheck = (Boolean)value;

        if (IsInvert)
        {
            isCheck = !isCheck;
        }

        return isCheck ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is Visibility))
        {
            throw new ArgumentException("The input variable has wrong type.");
        }

        bool result = (Visibility)value == Visibility.Visible;
        return (IsInvert) ? !result : result;
    }
}