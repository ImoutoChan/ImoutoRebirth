﻿using System.Windows;
using System.Windows.Data;
using ImoutoViewer.Extensions;

namespace ImoutoViewer.Converters;

internal class ConditionVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var result = Visibility.Collapsed;
        if (parameter != null)
        {
            var paramsConv = parameter.ToString();
            var paramsList = paramsConv.Split('|');

            if (BooleanResultConverter.CheckedValue(value, paramsList[0]))
            {
                result = Visibility.Visible;
            }
            else if (paramsList.Count() >= 2 && BooleanResultConverter.CheckedValue(value, paramsList[1]))
            {
                result = Visibility.Hidden;
            }
            else if (paramsList.Count() >= 3)
            {
                if (!BooleanResultConverter.CheckedValue(value, paramsList[2]))
                {
                    result = Visibility.Visible;
                }
            }
        }
        else if (Converts.To<object>(value) != null)
        {
            result = Visibility.Visible;

        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}