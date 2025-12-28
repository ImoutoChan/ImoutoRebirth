using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

[ValueConversion(typeof(string), typeof(string))]
internal class FileExtensionFromPathConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string fileName || string.IsNullOrEmpty(fileName))
            return string.Empty;

        var extension = Path.GetExtension(fileName);
        return string.IsNullOrEmpty(extension) 
            ? string.Empty 
            : extension.TrimStart('.').ToUpperInvariant();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}
