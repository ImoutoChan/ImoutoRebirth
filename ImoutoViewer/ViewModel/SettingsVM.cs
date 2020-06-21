using AssociationManager;
using Imouto.Viewer.Commands;
using Imouto.Viewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ControlzEx.Theming;
using ImoutoViewer.Properties;
using MahApps.Metro.Controls.Dialogs;

namespace Imouto.Viewer.ViewModel
{
    class SettingsVM : VMBase
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

            SaveCommand = new RelayCommand(x => Save());

            ShowTags = Settings.Default.ShowTags;

            ShowNotes = Settings.Default.ShowNotes;

            SetFileAssociationsCommand = new RelayCommand(param => SetFileAssociations());
        }

        #endregion Constructors

        #region Properties

        public int SlideshowDelay
        {
            get
            {
                return Settings.Default.SlideshowDelay;
            }
            set
            {
                Settings.Default.SlideshowDelay = value;
                OnPropertyChanged("SlideshowDelay");
            }
        }

        public List<ResizeTypeDescriptor> ResizeTypes { get; private set; }
        public ResizeTypeDescriptor SelectedResizeType
        {
            get
            {
                return _selectedResizeType;
            }
            set
            {
                _selectedResizeType = value;
                Settings.Default.ResizeType = value.Type;
                OnSelectedResizeTypeChanged();
            }
        }

        public ObservableCollection<DirectorySearchTypeDescriptor> DirectorySearchTypes { get; private set; }
        public DirectorySearchFlags DirectorySearchFlags
        {
            get
            {
                return DirectorySearchTypes.Where(item => item.IsSelected).Aggregate(DirectorySearchFlags.None, (current, item) => current | item.Type);
            }
        }

        public List<SortingDescriptor> FilesSortingMethods { get; set; }

        public List<SortingDescriptor> FoldersSortingMethods { get; set; }

        public SortingDescriptor SelectedFoldersSorting
        {
            get { return _selectedFoldersSorting; }
            set
            {
                _selectedFoldersSorting = value;
                Settings.Default.FoldersSorting = value.Method;
                OnSelectedFoldersSortingChanged();
            }
        }

        public bool IsSelectedFoldersSortingDescending
        {
            get { return _isSelectedFoldersSortingDescending; }
            set
            {
                _isSelectedFoldersSortingDescending = value;
                Settings.Default.FoldersSortingDesc = value;
                OnSelectedFoldersSortingChanged();
            }
        }

        public SortingDescriptor SelectedFilesSorting
        {
            get { return _selectedFilesSorting; }
            set
            {
                _selectedFilesSorting = value;
                Settings.Default.FilesSorting = value.Method;
                OnSelectedFilesSortingChanged();
            }
        }

        public bool IsSelectedFilesSortingDescending
        {
            get { return _isSelectedFilesSortingDescending; }
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
        /// Index of the selected theme. 0 - light, 1 - dark.
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
            get
            {
                return _showTags;
            }
            set
            {
                OnPropertyChanged(ref _showTags, value, () => ShowTags);
                Settings.Default.ShowTags = _showTags;
                OnSelectedTagsModeChanged();
            }
        }

        public bool ShowNotes
        {
            get
            {
                return _showNotes;
            }
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

        private async Task SetFileAssociations()
        {
            try
            {
                using (var mgr = new FileAssociationManager())
                {
                    foreach (
                        var ext in Extensions.GetSupportedFormatsList(typeof(ImageFormat)).Select(x => x.Substring(1)))
                    {
                        Associate(mgr, ext);
                    }
                }

                await MainWindow.CurrentWindow.ShowMessageAsync("File association",
                    "Associations are successfully set.",
                    MessageDialogStyle.AffirmativeAndNegative);
            }
            catch (UnauthorizedAccessException e)
            {
                var result = await MainWindow.CurrentWindow.ShowMessageAsync("File association", 
                    "You need administrative rigths to set assotiations. Application will be restarted with admistrative rights.",
                    MessageDialogStyle.AffirmativeAndNegative);

                if (result == MessageDialogResult.Affirmative)
                {
                    // Launch itself as administrator 
                    var proc = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Assembly.GetExecutingAssembly().Location,
                        Verb = "runas"
                    };


                    try
                    {
                        Process.Start(proc);
                    }
                    catch
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
            string executablePath = Assembly.GetExecutingAssembly().Location;

            using (var ext = mgr.RegisterFileAssociation(extension))
            {
                ext.DefaultIcon = new ApplicationIcon(executablePath);
                ext.ShellOpenCommand = executablePath;
                ext.Associated = true;
            }
        }

        #endregion Methods

        #region Event handlers

        private void item_SelectedChanged(object sender, EventArgs e)
        {
            OnSelectedDirectorySearchTypeChanged();
        }

        #endregion  Event handlers

        #region Events

        public event EventHandler SelectedResizeTypeChanged;
        private void OnSelectedResizeTypeChanged()
        {
            if (SelectedResizeTypeChanged != null)
            {
                SelectedResizeTypeChanged(this, null);
            }
        }

        public event EventHandler SelectedDirectorySearchTypeChanged;
        private void OnSelectedDirectorySearchTypeChanged()
        {
            if (SelectedDirectorySearchTypeChanged != null)
            {
                SelectedDirectorySearchTypeChanged(this, null);
            }
        }

        public event EventHandler SelectedFilesSortingChanged;
        private void OnSelectedFilesSortingChanged()
        {
            if (SelectedFilesSortingChanged != null)
            {
                SelectedFilesSortingChanged(this, null);
            }
        }

        public event EventHandler SelectedFoldersSortingChanged;
        private void OnSelectedFoldersSortingChanged()
        {
            if (SelectedFoldersSortingChanged != null)
            {
                SelectedFoldersSortingChanged(this, null);
            }
        }

        public event EventHandler SelectedTagsModeChanged;
        private void OnSelectedTagsModeChanged()
        {
            if (SelectedTagsModeChanged != null)
            {
                SelectedTagsModeChanged(this, null);
            }
        }

        public event EventHandler SelectedNotesModeChanged;
        private void OnSelectedNotesModeChanged()
        {
            var handler = SelectedNotesModeChanged;
            handler?.Invoke(this, null);
        }

        
        #endregion Events
    }

    class ResizeTypeDescriptor
    {
        #region Properties

        public string Name { get; private set; }
        public ResizeType Type { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion Methods

        #region Static methods

        public static List<ResizeTypeDescriptor> GetList()
        {
            return new List<ResizeTypeDescriptor>
            {
                new ResizeTypeDescriptor { Name = "Fit to screen (downscale only)", Type = ResizeType.DownscaleToViewPort },
                new ResizeTypeDescriptor { Name = "Fit to screen (down & up scale)", Type = ResizeType.FitToViewPort },
                new ResizeTypeDescriptor { Name = "Fit to screen width (downscale only)", Type = ResizeType.DownscaleToViewPortWidth },
                new ResizeTypeDescriptor { Name = "Fit to screen width (down & up scale)", Type = ResizeType.FitToViewPortWidth },
                new ResizeTypeDescriptor { Name = "Original size (no resize)", Type = ResizeType.NoResize },
            };
        }

        #endregion Static methods
    }

    class DirectorySearchTypeDescriptor
    {
        #region Fields

        private bool? _isSelected;

        #endregion  Fields

        #region Properties

        public string Name { get; private set; }
        public DirectorySearchFlags Type { get; private set; }
        public bool IsSelected
        {
            get
            {
                if (_isSelected == null)
                {
                    _isSelected = ((Type & Settings.Default.DirectorySearchFlags) == Type);
                }
                return (bool)_isSelected;
            }
            set
            {
                _isSelected = value;
                Settings.Default.DirectorySearchFlags = (value)
                    ? Settings.Default.DirectorySearchFlags | Type
                    : Settings.Default.DirectorySearchFlags & ~Type;
                OnSelectedChanged();
            }
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion Methods

        #region Static methods

        public static ObservableCollection<DirectorySearchTypeDescriptor> GetList()
        {
            return new ObservableCollection<DirectorySearchTypeDescriptor>
            {
                new DirectorySearchTypeDescriptor { Name = "All Pre", Type = DirectorySearchFlags.AllDepthPrefolder },
                new DirectorySearchTypeDescriptor { Name = "Pre", Type = DirectorySearchFlags.Prefolders },
                new DirectorySearchTypeDescriptor { Name = "Cur", Type = DirectorySearchFlags.Folder },
                new DirectorySearchTypeDescriptor { Name = "Sub", Type = DirectorySearchFlags.Subfolders },
                new DirectorySearchTypeDescriptor { Name = "All Sub", Type = DirectorySearchFlags.AllDepthSubfolders },
            };
        }

        #endregion Static methods

        #region Events

        public event EventHandler SelectedChanged;
        private void OnSelectedChanged()
        {
            if (SelectedChanged != null)
            {
                SelectedChanged(this, new EventArgs());
            }
        }

        #endregion Events
    }

    class SortingDescriptor
    {
        #region Properties

        public string Name { get; set; }
        public SortMethod Method { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion Methods

        #region Static methods

        public static List<SortingDescriptor> GetListForFiles()
        {
            return new List<SortingDescriptor>
            {
                new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
                new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate  },
                new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate },
                new SortingDescriptor { Name = "Size", Method = SortMethod.BySize }
            };
        }

        public static List<SortingDescriptor> GetListForFolders()
        {
            return new List<SortingDescriptor>
            {
                new SortingDescriptor { Name = "Name", Method = SortMethod.ByName },
                new SortingDescriptor { Name = "Date created", Method = SortMethod.ByCreateDate  },
                new SortingDescriptor { Name = "Date modified", Method = SortMethod.ByUpdateDate }
            };
        }

        #endregion Static methods
    }

    class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush ColorBrush { get; set; }

        public void ChangeAccent()
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
            Settings.Default.AccentColorName = Name;
        }
    }

    class TagsModeDescriptor
    {
        #region Properties

        public string Name { get; set; }
        public byte Value { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion Methods

        #region Static methods

        public static List<TagsModeDescriptor> GetListForFiles()
        {
            return new List<TagsModeDescriptor>
            {
                new TagsModeDescriptor { Name = "Show", Value = 1 },
                new TagsModeDescriptor { Name = "Hide", Value = 0  },
            };
        }

        #endregion Static methods
    }

}
