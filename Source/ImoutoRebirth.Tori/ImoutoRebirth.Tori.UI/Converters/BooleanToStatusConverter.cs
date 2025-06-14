using System;
using System.Globalization;
using System.Windows.Data;

namespace ImoutoRebirth.Tori.UI.Converters;

public class BooleanToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Installed" : "Not Installed";
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 