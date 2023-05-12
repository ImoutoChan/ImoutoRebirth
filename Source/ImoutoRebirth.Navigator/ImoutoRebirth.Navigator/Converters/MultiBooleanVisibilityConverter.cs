using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Converters;

public class MultiBooleanVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = values.OfType<bool>().All(x => x) ? Visibility.Visible : Visibility.Collapsed;
        return result;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
