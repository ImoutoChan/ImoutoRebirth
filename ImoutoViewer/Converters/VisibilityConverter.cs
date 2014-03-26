using System;
using System.Windows;
using System.Windows.Data;

namespace ImoutoViewer.Converters
{
    [ValueConversion(typeof(Visibility), typeof(Boolean))]
    class VisibilityConverter : IValueConverter
    {
        public VisibilityConverter()
        {
            IsInvert = false;
            CollapsedOnFalse = false;
        }

        public bool IsInvert { private get; set; }
        public bool CollapsedOnFalse { private get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Boolean)) throw new ArgumentException("The input variable has wrong type.");

            var isCheck = (Boolean)value;

            if (IsInvert)
            {
                isCheck = !isCheck;
            }

            return isCheck
                       ? Visibility.Visible
                       : CollapsedOnFalse
                             ? Visibility.Collapsed
                             : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Visibility))
            {
                throw new ArgumentException("The input variable has wrong type.");
            }

            bool result = (Visibility) value == Visibility.Visible;
            return (IsInvert)
                       ? !result
                       : result;
        }
    }
}
