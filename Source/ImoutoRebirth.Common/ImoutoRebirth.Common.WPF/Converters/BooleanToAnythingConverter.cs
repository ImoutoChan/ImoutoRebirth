using System.Globalization;
using System.Windows.Data;

namespace ImoutoRebirth.Common.WPF.Converters;

public class BooleanToAnythingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null) 
            return value;
        
        var paramsConv = parameter.ToString()!;
        var paramsList = paramsConv.Split('|');

        // value to compare to | true result | false result

        if (BooleanResultConverter.CheckedValue(value, paramsList[0]))
        {
            if (double.TryParse(paramsList[1], out var doubleResult))
                return doubleResult;

            return paramsList[1];
        }
        else
        {
            if (double.TryParse(paramsList[2], out var doubleResult))
                return doubleResult;

            return paramsList[2];
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
