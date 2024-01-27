using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Common.WPF.Converters;

/// <summary>
/// param: true|false|!null
/// param[0] compared to value and sets result to visible if it's match
/// param[1] compared to value and sets result to hidden if it's match
/// param[2] compared to value and sets result to visible if it's not match (I don't know why...)
/// </summary>
public class ConditionVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = Visibility.Collapsed;
        if (parameter != null)
        {
            var paramsConv = parameter.ToString()!;
            var paramsList = paramsConv.Split('|');

            if (BooleanResultConverter.CheckedValue(value, paramsList[0]))
            {
                result = Visibility.Visible;
            }
            else if (paramsList.Length >= 2 && BooleanResultConverter.CheckedValue(value, paramsList[1]))
            {
                result = Visibility.Hidden;
            }
            else if (paramsList.Length >= 3 && !BooleanResultConverter.CheckedValue(value, paramsList[2]))
            {
                result = Visibility.Visible;
            }
        }
        else if (Converts.To<object>(value) != null)
        {
            result = Visibility.Visible;
        }
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
