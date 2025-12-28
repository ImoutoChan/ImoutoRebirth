using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

public static class ThemeService
{
    private static readonly Color LightBackground 
        = (Color)ColorConverter.ConvertFromString("#FFFFD9D9");
    private static readonly Color LightForeground 
        = (Color)ColorConverter.ConvertFromString("#FF501E3C");

    private static readonly Color FavoriteAccentLight
        = (Color)ColorConverter.ConvertFromString("#DA2023");
    private static readonly Color FavoriteAccentDark
        = (Color)ColorConverter.ConvertFromString("#E64C8A");

    public static void Initialize(Application application)
    {
        UpdateFileInfoNiceBrushes(application, ThemeManager.Current.DetectTheme(application));
        ThemeManager.Current.ThemeChanged += (_, e) => UpdateFileInfoNiceBrushes(application, e.NewTheme);
    }

    private static void UpdateFileInfoNiceBrushes(Application application, Theme? theme)
    {
        var isDark = theme?.BaseColorScheme == "Dark";

        application.Resources["FileInfoNice.Background"] = new SolidColorBrush(isDark ? LightForeground : LightBackground);
        application.Resources["FileInfoNice.Foreground"] = new SolidColorBrush(isDark ? LightBackground : LightForeground);
        application.Resources["FileInfoNice.FavoriteAccent"] = new SolidColorBrush(isDark ? FavoriteAccentDark : FavoriteAccentLight);
    }
}
