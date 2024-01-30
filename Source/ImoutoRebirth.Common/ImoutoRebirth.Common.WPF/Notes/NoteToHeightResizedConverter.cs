using System.Globalization;
using System.Windows.Data;

namespace ImoutoRebirth.Common.WPF.Notes;

public class NoteToHeightResizedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var noteHeight = values[1] as int?;
        var zoom = values[0] as double? ?? 1;

        return noteHeight * zoom ?? 0;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
