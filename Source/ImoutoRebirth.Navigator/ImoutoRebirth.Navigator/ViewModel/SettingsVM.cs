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

        public void ChangeAccent(IReadOnlyCollection<AccentColorMenuData>? randomColors = null)
        {
            if (randomColors?.Any() == true)
            {
                var colors = randomColors.Where(x => x.Name != "Random").ToList();
                var randomColor = colors[Random.Shared.Next(colors.Count)];

                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, randomColor.Name);
                Settings.Default.AccentColorName = "Random";
            }
            else
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
                Settings.Default.AccentColorName = Name;
            }
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
            })
            .Append(new AccentColorMenuData
            {
                Name = "Random",
                ColorBrush = Brushes.Black
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
        get => Settings.Default.ActivatePreviewOnSelect;
        set
        {
            Settings.Default.ActivatePreviewOnSelect = value;
            OnPropertyChanged(() => ShowPreviewOnSelect);
            OnShowPreviewOnSelectChanged();
        }
    }

    public bool ShowSystemTags
    {
        get => Settings.Default.ShowSystemTags;
        set
        {
            Settings.Default.ShowSystemTags = value;
            OnPropertyChanged(() => ShowSystemTags);
        }
    }

    public List<AccentColorMenuData> AccentColors { get; }

    public AccentColorMenuData SelectedAccentColor
    {
        get => _selectedAccentColor;
        set
        {
            _selectedAccentColor = value;

            if (_selectedAccentColor.Name == "Random")
            {
                _selectedAccentColor.ChangeAccent(AccentColors);
            }
            else
            {
                _selectedAccentColor.ChangeAccent();
            }
        }
    }

    /// <summary>
    ///     Index of the selected theme. 0 - light, 1 - dark.
    /// </summary>
    public int SelectedIndexTheme
    {
        get => _selectedTheme;
        set
        {
            _selectedTheme = value;
            var theme = ThemeManager.Current.DetectTheme(Application.Current);
            switch (value)
            {
                case 2:
                    var next = Random.Shared.Next(10);
                    var name = next % 2 == 0 ? "Dark" : "Light";
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, name);
                    Settings.Default.ThemeIndex = 2;
                    break;
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

    public string LilinHost
    {
        get => Settings.Default.LilinHost;
        set => Settings.Default.LilinHost = value;
    }

    public string RoomHost
    {
        get => Settings.Default.RoomHost;
        set => Settings.Default.RoomHost = value;
    }

    #endregion Properties

    #region Commands

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(x => Save());

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
        handler?.Invoke(this, EventArgs.Empty);
    }

    #endregion Events
}

public record PathOverride(string OldPathPattern, string NewPathPart);
