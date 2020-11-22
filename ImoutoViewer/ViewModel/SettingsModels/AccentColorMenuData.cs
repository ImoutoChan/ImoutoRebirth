using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;

namespace ImoutoViewer.ViewModel.SettingsModels
{
    class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush ColorBrush { get; set; }

        public void ChangeAccent()
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
            Properties.Settings.Default.AccentColorName = Name;
        }
    }
}