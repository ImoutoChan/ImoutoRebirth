using System.Windows.Media;

namespace ImoutoViewer.Extensions;

public static class ColorExtensions
{
    public static int ToInt32(this Color color)
    {
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }

    public static Color ToColor(this int number)
    {
        return Color.FromArgb(((byte)(number >> 24) == 0) ? (byte)255 : (byte)(number >> 24),
            (byte)(number >> 16),
            (byte)(number >> 8),
            (byte)(number));
    }
}