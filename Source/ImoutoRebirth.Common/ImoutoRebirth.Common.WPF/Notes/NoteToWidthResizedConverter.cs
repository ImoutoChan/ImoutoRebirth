using System.Windows.Data;

namespace ImoutoRebirth.Common.WPF.Notes;

public class NoteToWidthResizedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var noteWidth = values[1] as int?;
        var zoom = values[0] as double? ?? 1;

        return noteWidth * zoom ?? 0;
    }

    public object[] ConvertBack(
        object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture) =>
        throw new NotImplementedException();
}
