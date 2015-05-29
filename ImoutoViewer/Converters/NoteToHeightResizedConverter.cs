using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ImoutoViewer.Model;
using ImoutoViewer.ViewModel;
using Utils;

namespace ImoutoViewer.Converters
{
    class NoteToHeightResizedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var noteM = values[1] as NoteM;
            double zoom = values[0] as double? ?? 1;

            return noteM.Height * zoom;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
