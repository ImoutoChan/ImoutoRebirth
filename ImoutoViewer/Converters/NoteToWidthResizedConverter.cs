using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Imouto.Viewer.Model;
using Imouto.Viewer.ViewModel;
using Imouto.Utils;

namespace Imouto.Viewer.Converters
{
    class NoteToWidthResizedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var noteM = values[1] as NoteM;
            double zoom = values[0] as double? ?? 1;

            return noteM.Width * zoom;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
