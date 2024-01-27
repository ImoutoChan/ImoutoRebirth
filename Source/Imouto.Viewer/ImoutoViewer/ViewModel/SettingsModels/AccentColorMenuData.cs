using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;

namespace ImoutoViewer.ViewModel.SettingsModels;

internal class AccentColorMenuData
{
    public required string Name { get; init; }
    public required Brush ColorBrush { get; init; }

    public void ChangeAccent()
    {
        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
        Properties.Settings.Default.AccentColorName = Name;
    }
}
