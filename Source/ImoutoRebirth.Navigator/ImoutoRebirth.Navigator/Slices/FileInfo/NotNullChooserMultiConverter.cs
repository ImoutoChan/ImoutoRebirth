using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

/// <summary>
/// Returns the first non-null value from the bound values, converted to string if targeting a string property.
/// For Visibility, returns Visible if any value is Visible.
/// </summary>
internal class NotNullChooserMultiConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(Visibility))
        {
            return values.Any(x => x is Visibility.Visible)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        foreach (var value in values)
        {
            if (value == null || value == DependencyProperty.UnsetValue) 
                continue;

            if (targetType == typeof(string))
                return value.ToString();
                
            return value;
        }

        return null;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
