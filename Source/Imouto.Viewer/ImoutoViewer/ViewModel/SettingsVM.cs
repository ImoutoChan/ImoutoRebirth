﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using ControlzEx.Theming;
using ImoutoRebirth.Common.WindowsAssociationManager;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoViewer.Model;
using ImoutoViewer.Properties;
using ImoutoViewer.ViewModel.SettingsModels;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoViewer.ViewModel;

internal class SettingsVM : VMBase
{
    #region Fields

    private ResizeTypeDescriptor _selectedResizeType;
    private SortingDescriptor _selectedFilesSorting;
    private SortingDescriptor _selectedFoldersSorting;
    private bool _isSelectedFoldersSortingDescending;
    private bool _isSelectedFilesSortingDescending;
    private AccentColorMenuData _selectedAccentColor;
    private int _selectedTheme;
    private bool _showTags;
    private bool _showNotes;

    #endregion Fileds

    #region Constructors

    public SettingsVM()
    {
        ResizeTypes = ResizeTypeDescriptor.GetList();
        SelectedResizeType = ResizeTypes.First(x => x.Type == Settings.Default.ResizeType);

        DirectorySearchTypes = DirectorySearchTypeDescriptor.GetList();
        foreach (var item in DirectorySearchTypes)
        {
            item.SelectedChanged += item_SelectedChanged;
        }

        FilesSortingMethods = SortingDescriptor.GetListForFiles();
        FoldersSortingMethods = SortingDescriptor.GetListForFolders();

        SelectedFoldersSorting = FoldersSortingMethods.First(x => x.Method == Settings.Default.FoldersSorting);
        SelectedFilesSorting = FilesSortingMethods.First(x => x.Method == Settings.Default.FilesSorting);

        IsSelectedFilesSortingDescending = Settings.Default.FilesSortingDesc;
        IsSelectedFoldersSortingDescending = Settings.Default.FoldersSortingDesc;

        AccentColors = ThemeManager.Current.Themes
            .Select(a => new AccentColorMenuData
            {
                Name = a.Name.Split('.').Last(),
                ColorBrush = a.ShowcaseBrush
            })
            .ToList();
        SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);

        SelectedIndexTheme = Settings.Default.ThemeIndex;

        SaveCommand = new RelayCommand(_ => Save());

        ShowTags = Settings.Default.ShowTags;

        ShowNotes = Settings.Default.ShowNotes;

        SetFileAssociationsCommand = new AsyncCommand(SetFileAssociations);
    }

    #endregion Constructors

    #region Properties

    public int SlideshowDelay
    {
        get => Settings.Default.SlideshowDelay;
        set
        {
            Settings.Default.SlideshowDelay = value;
            OnPropertyChanged("SlideshowDelay");
        }
    }

    public List<ResizeTypeDescriptor> ResizeTypes { get; private set; }
    public ResizeTypeDescriptor SelectedResizeType
    {
        get => _selectedResizeType;
        [MemberNotNull(nameof(_selectedResizeType))]
        set
        {
            _selectedResizeType = value;
            Settings.Default.ResizeType = value.Type;
            OnSelectedResizeTypeChanged();
        }
    }

    public ObservableCollection<DirectorySearchTypeDescriptor> DirectorySearchTypes { get; private set; }
    public DirectorySearchFlags DirectorySearchFlags 
        => DirectorySearchTypes
            .Where(item => item.IsSelected)
            .Aggregate(DirectorySearchFlags.None, (current, item) => current | item.Type);

    public List<SortingDescriptor> FilesSortingMethods { get; set; }

    public List<SortingDescriptor> FoldersSortingMethods { get; set; }

    public SortingDescriptor SelectedFoldersSorting
    {
        get => _selectedFoldersSorting;
        [MemberNotNull(nameof(_selectedFoldersSorting))]
        set
        {
            _selectedFoldersSorting = value;
            Settings.Default.FoldersSorting = value.Method;
            OnSelectedFoldersSortingChanged();
        }
    }

    public bool IsSelectedFoldersSortingDescending
    {
        get => _isSelectedFoldersSortingDescending;
        set
        {
            _isSelectedFoldersSortingDescending = value;
            Settings.Default.FoldersSortingDesc = value;
            OnSelectedFoldersSortingChanged();
        }
    }

    public SortingDescriptor SelectedFilesSorting
    {
        get => _selectedFilesSorting;
        [MemberNotNull(nameof(_selectedFilesSorting))]
        set
        {
            _selectedFilesSorting = value;
            Settings.Default.FilesSorting = value.Method;
            OnSelectedFilesSortingChanged();
        }
    }

    public bool IsSelectedFilesSortingDescending
    {
        get => _isSelectedFilesSortingDescending;
        set
        {
            _isSelectedFilesSortingDescending = value;
            Settings.Default.FilesSortingDesc = value;
            OnSelectedFilesSortingChanged();
        }
    }

    public List<AccentColorMenuData> AccentColors { get; private set; }

    public AccentColorMenuData SelectedAccentColor
    {
        get => _selectedAccentColor;
        [MemberNotNull(nameof(_selectedAccentColor))]
        set
        {
            _selectedAccentColor = value;
            _selectedAccentColor.ChangeAccent();
        }
    }

    /// <summary>
    /// Index of the selected theme. 0 - light, 1 - dark.
    /// </summary>
    public int SelectedIndexTheme
    {
        get => _selectedTheme;
        set
        {
            _selectedTheme = value;
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

    public bool ShowTags
    {
        get => _showTags;
        set
        {
            OnPropertyChanged(ref _showTags, value, () => ShowTags);
            Settings.Default.ShowTags = _showTags;
            OnSelectedTagsModeChanged();
        }
    }

    public bool ShowNotes
    {
        get => _showNotes;
        set
        {
            OnPropertyChanged(ref _showNotes, value, () => ShowNotes);
            Settings.Default.ShowNotes = _showNotes;
            OnSelectedNotesModeChanged();
        }
    }

    #endregion Properties

    #region Commands

    public ICommand SaveCommand { get; set; }

    private void Save()
    {
        Settings.Default.Save();
    }

    public ICommand SetFileAssociationsCommand { get; private set; }

    #endregion Commands

    #region Methods

    private static async Task SetFileAssociations()
    {
        try
        {
            using (var mgr = new FileAssociationManager(
                       FileAssociationManager.AssociationLocation.CurrentUser,
                       "ImoutoRebirth", 
                       "ImoutoViewer",
                       "Image viewer app"))
            {
                foreach (var ext in SupportedFormatsExtensions
                             .GetSupportedFormatsList(typeof(ImageFormat))
                             .Select(x => x[1..]))
                {
                    Associate(mgr, ext);
                }
            }

            await MainWindow.CurrentWindow.ShowMessageAsync("File association",
                "Associations are successfully set.",
                MessageDialogStyle.AffirmativeAndNegative);
        }
        catch (UnauthorizedAccessException)
        {
            var result = await MainWindow.CurrentWindow.ShowMessageAsync("File association", 
                "You need administrative rights to set associations. Application will be restarted with administrative rights.",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                // Launch itself as administrator 
                var proc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Process.GetCurrentProcess().MainModule!.FileName,
                    Verb = "runas"
                };

                try
                {
                    Process.Start(proc);
                }
                catch (Exception)
                {
                    return;
                }

                Application.Current.Shutdown();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void Associate(FileAssociationManager mgr, string extension)
    {
        var executablePath = Process.GetCurrentProcess().MainModule!.FileName;

        using var ext = mgr.RegisterFileAssociation(extension);

        ext.DefaultIcon = new ApplicationIcon(executablePath);
        ext.ShellOpenCommand = executablePath;
        ext.Associated = true;
    }

    #endregion Methods

    #region Event handlers

    private void item_SelectedChanged(object? sender, EventArgs e) => OnSelectedDirectorySearchTypeChanged();

    #endregion  Event handlers

    #region Events

    public event EventHandler? SelectedResizeTypeChanged;
    private void OnSelectedResizeTypeChanged() => SelectedResizeTypeChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SelectedDirectorySearchTypeChanged;
    private void OnSelectedDirectorySearchTypeChanged() => SelectedDirectorySearchTypeChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SelectedFilesSortingChanged;
    private void OnSelectedFilesSortingChanged() => SelectedFilesSortingChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SelectedFoldersSortingChanged;
    private void OnSelectedFoldersSortingChanged() => SelectedFoldersSortingChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SelectedTagsModeChanged;
    private void OnSelectedTagsModeChanged() => SelectedTagsModeChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SelectedNotesModeChanged;
    private void OnSelectedNotesModeChanged() => SelectedNotesModeChanged?.Invoke(this, EventArgs.Empty);

    #endregion Events
}
