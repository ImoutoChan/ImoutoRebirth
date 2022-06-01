using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using ImoutoRebirth.Navigator.Commands;

namespace ImoutoRebirth.Navigator.ViewModel;

class SettingsVM : VMBase
{
    #region Subclasses

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush ColorBrush { get; set; }

        public void ChangeAccent()
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
            Settings.Default.AccentColorName = Name;
        }
    }

    #endregion Subclasses

    #region Fields

    private AccentColorMenuData _selectedAccentColor;
    private int _selectedTheme;
    private ICommand _saveCommand;
    private string _pathOverrides;

    #endregion Fileds

    #region Constructors

    public SettingsVM()
    {
        AccentColors = ThemeManager.Current.Themes
            .GroupBy(x => x.ColorScheme)
            .OrderBy(x => x.Key)
            .Select(a => new AccentColorMenuData
            {
                Name = a.Key,
                ColorBrush = a.First().ShowcaseBrush
            }).ToList();

        SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);

        SelectedIndexTheme = Settings.Default.ThemeIndex;

        ShowPreviewOnSelect = Settings.Default.ActivatePreviewOnSelect;
        PathOverrides = Settings.Default.PathOverrides;
    }

    #endregion Constructors

    #region Properties

    public bool ShowPreviewOnSelect
    {
        get
        {
            return Settings.Default.ActivatePreviewOnSelect;
        }
        set
        {
            Settings.Default.ActivatePreviewOnSelect = value;
            OnPropertyChanged(() => ShowPreviewOnSelect);
            OnShowPreviewOnSelectChanged();
        }
    }

    public List<AccentColorMenuData> AccentColors { get; }

    public AccentColorMenuData SelectedAccentColor
    {
        get
        {
            return _selectedAccentColor;
        }
        set
        {
            _selectedAccentColor = value;
            _selectedAccentColor.ChangeAccent();
        }
    }

    /// <summary>
    ///     Index of the selected theme. 0 - light, 1 - dark.
    /// </summary>
    public int SelectedIndexTheme
    {
        get
        {
            return _selectedTheme;
        }
        set
        {
            _selectedTheme = value;
            var theme = ThemeManager.Current.DetectTheme(Application.Current);
            switch (value)
            {
                case 1:
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
                    Settings.Default.ThemeIndex = 1;
                    break;
                default:
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
                    Settings.Default.ThemeIndex = 0;
                    break;
            }
        }
    }
    
    
    /// <summary>
    /// Format: "path1pattern->path1new;;;path2pattern->path2new"
    /// </summary>
    public string PathOverrides
    {
        get => _pathOverrides;
        set
        {
            _pathOverrides = value;
            Settings.Default.PathOverrides = value;
        }
    }

    #endregion Properties

    #region Commands

    public ICommand SaveCommand
    {
        get
        {
            return _saveCommand ?? (_saveCommand = new RelayCommand(x => Save()));
        }
    }

    private void Save()
    {
        Settings.Default.Save();
    }

    #endregion Commands

    #region Events

    public event EventHandler ShowPreviewOnSelectChanged;

    private void OnShowPreviewOnSelectChanged()
    {
        var handler = ShowPreviewOnSelectChanged;
        handler?.Invoke(this, new EventArgs());
    }

    #endregion Events
}

public record PathOverride(string OldPathPattern, string NewPathPart);
