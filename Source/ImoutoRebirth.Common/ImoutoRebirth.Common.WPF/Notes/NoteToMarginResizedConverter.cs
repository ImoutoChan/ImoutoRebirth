using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImoutoRebirth.Common.WPF.Notes;

public class NoteToMarginResizedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var positionFromLeft = values[1] as int?;
        var positionFromTop = values[2] as int?;
        var zoom = values[0] as double? ?? 1;
        var rootVisual = values[3] as FrameworkElement;
        var viewPort = values[4] as FrameworkElement;

        if (positionFromLeft == null || positionFromTop == null || rootVisual == null || viewPort == null)
            return new Thickness(0, 0, 0, 0);
        
        var relativePoint = viewPort.TransformToAncestor(rootVisual).Transform(new Point(0, 0));
        return new Thickness(
            positionFromLeft.Value * zoom + relativePoint.X,
            positionFromTop.Value * zoom + relativePoint.Y, 
            0, 
            0);
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
