using System.Globalization;
using System.Windows.Data;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.Converters;

internal class BooleanResultConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null) 
            return false;

        var paramsConv = parameter.ToString();
        return CheckedValue(value, paramsConv);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();

    public static bool CheckedValue(object? value, string? param)
    {
        if (string.IsNullOrWhiteSpace(param))
            return false;

        var paramToLower = param.Trim().ToLowerInvariant();
        switch (paramToLower)
        {
            case "true":
                return Converts.To<bool?>(value) == true;
            case "false":
                return Converts.To<bool?>(value) == false;
            case "null":
                return Converts.To<object>(value) == null;
            case "!true":
                return Converts.To<bool?>(value) != true;
            case "!false":
                return Converts.To<bool?>(value) != false;
            case "!null":
                return Converts.To<object>(value) != null;
            default:
            {
                var valueStr = Converts.To<string>(value);

                if (param.StartsWith('!'))
                {
                    param = param.Remove(0, 1);
                    return valueStr != param;
                }
                else
                {
                    return valueStr == param;
                }
            }
        }
    }
}
