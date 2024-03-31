using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using MahApps.Metro.Theming;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class SettingsVM : VMBase
{
    private AccentColorMenuData _selectedAccentColor;
    private int _selectedTheme;
    private ICommand? _saveCommand;
    private ICommand? _toggleImoutoPicsCommand;
    private string _pathOverrides;
    private readonly IImoutoPicsUploaderStateService _imoutoPicsUploaderStateService;
    private bool _isImoutoPicsUploaderEnabled;

    public SettingsVM()
    {
        AccentColors = ThemeManager.Current.Themes
            .GroupBy(x => x.ColorScheme)
            .OrderBy(x => x.Key)
            .Select(a => new AccentColorMenuData(a.Key, a.First().ShowcaseBrush))
            .Append(new AccentColorMenuData("Random", Brushes.Black))
            .Append(new CustomTheme("Elite", new SolidColorBrush(Color.FromRgb(0xff, 0xcb, 0x74))))
            .ToList();

        SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);
        SelectedIndexTheme = Settings.Default.ThemeIndex;

        ShowPreviewOnSelect = Settings.Default.ActivatePreviewOnSelect;
        PathOverrides = Settings.Default.PathOverrides;

        _imoutoPicsUploaderStateService = ServiceLocator.GetService<IImoutoPicsUploaderStateService>();
    }

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
    
    public bool AutoShuffle
    {
        get => Settings.Default.AutoShuffle;
        set
        {
            Settings.Default.AutoShuffle = value;
            OnPropertyChanged(() => AutoShuffle);
        }
    }

    public List<AccentColorMenuData> AccentColors { get; }

    public AccentColorMenuData SelectedAccentColor
    {
        get => _selectedAccentColor;
        
        [MemberNotNull(nameof(_selectedAccentColor))]
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
        
        [MemberNotNull(nameof(_pathOverrides))]
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

    public bool IsImoutoPicsUploaderEnabled
    {
        get => _isImoutoPicsUploaderEnabled;
        set => OnPropertyChanged(ref _isImoutoPicsUploaderEnabled, value, () => IsImoutoPicsUploaderEnabled);
    }

    public string RoomHost
    {
        get => Settings.Default.RoomHost;
        set => Settings.Default.RoomHost = value;
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(_ => Save());

    public ICommand ToggleImoutoPicsCommand => _toggleImoutoPicsCommand ??= new AsyncCommand(() => ToggleImoutoPics());

    private static void Save() => Settings.Default.Save();

    public event EventHandler? ShowPreviewOnSelectChanged;

    private void OnShowPreviewOnSelectChanged()
    {
        var handler = ShowPreviewOnSelectChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeAsync()
    {
        IsImoutoPicsUploaderEnabled = await _imoutoPicsUploaderStateService.IsEnabledAsync();
    }
    
    private async Task ToggleImoutoPics()
    {
        if (IsImoutoPicsUploaderEnabled)
        {
            await _imoutoPicsUploaderStateService.DisableAsync();
        }
        else
        {
            await _imoutoPicsUploaderStateService.EnableAsync();
        }
        
        IsImoutoPicsUploaderEnabled = await _imoutoPicsUploaderStateService.IsEnabledAsync();
    }
}

public class AccentColorMenuData
{
    public AccentColorMenuData(string name, Brush colorBrush)
    {
        Name = name;
        ColorBrush = colorBrush;
    }

    public string Name { get; private set; }

    public Brush ColorBrush { get; private set; }

    public virtual void ChangeAccent(IReadOnlyCollection<AccentColorMenuData>? randomColors = null)
    {
        if (randomColors.SafeAny())
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
    
public class CustomTheme : AccentColorMenuData
{
    public CustomTheme(string name, Brush colorBrush) : base(name, colorBrush)
    {
    }

    public override void ChangeAccent(IReadOnlyCollection<AccentColorMenuData>? randomColors = null)
    {
        if (ThemeManager.Current.Themes.All(x => x.Name != Name))
        {
            var darkTheme = ThemeManager.Current.AddLibraryTheme(
                new LibraryTheme(
                    new Uri(
                        $"pack://application:,,,/ImoutoRebirth.Navigator;component/Themes/ColorSchemes/Dark.{Name}.xaml"),
                    MahAppsLibraryThemeProvider.DefaultInstance
                )
            );
            var lightTheme = ThemeManager.Current.AddLibraryTheme(
                new LibraryTheme(
                    new Uri(
                        $"pack://application:,,,/ImoutoRebirth.Navigator;component/Themes/ColorSchemes/Light.{Name}.xaml"),
                    MahAppsLibraryThemeProvider.DefaultInstance
                )
            );
        }

        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
        Settings.Default.AccentColorName = Name;
    }
}
