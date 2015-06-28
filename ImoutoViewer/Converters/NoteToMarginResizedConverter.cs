using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Imouto.Viewer.Model;

namespace Imouto.Viewer.Converters
{
    class NoteToMarginResizedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var noteM = values[1] as NoteM;
            double zoom = values[0] as double? ?? 1;
            var rootVisual = values[2] as FrameworkElement;
            var myVisual = values[3] as FrameworkElement;

            Point relativePoint = myVisual.TransformToAncestor(rootVisual).Transform(new Point(0, 0));

            return new Thickness(noteM.PositionX * zoom + relativePoint.X, noteM.PositionY * zoom + relativePoint.Y, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
