using System.Text.RegularExpressions;

namespace ImoutoViewer.Extensions;

public static partial class StringExtensions
{
    public static string GetAlphanumericName(this string fileName)
    {
        var regex = NumbersRegex();
        return regex.Replace(fileName, match => match.Value.PadLeft(50, '0'));
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumbersRegex();
}
