using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport;

public class FilePathToFileNameConverter : IValueConverter
{
    public static FilePathToFileNameConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string filePath && !string.IsNullOrEmpty(filePath))
        {
            var extension = Path.GetExtension(filePath);
            return string.IsNullOrEmpty(extension)
                ? Path.GetFileName(filePath)
                : extension.TrimStart('.').ToUpperInvariant();
        }

        return value ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
