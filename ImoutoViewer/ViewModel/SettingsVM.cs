using System.Windows.Input;
using ImoutoViewer.Commands;
using ImoutoViewer.Model;
using ImoutoViewer.Properties;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ImoutoViewer.ViewModel
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

        #endregion //Fileds
        
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

            AccentColors = ThemeManager.DefaultAccents
                                .Select(a => new AccentColorMenuData
                                {
                                    Name = a.Name, 
                                    ColorBrush = a.Resources["AccentColorBrush"] as Brush
                                })
                                .ToList();
            SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);

            SelectedIndexTheme = Settings.Default.ThemeIndex;

            SaveCommand = new RelayCommand(x=> Save());
        }

        #endregion //Constructors

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
                var theme = ThemeManager.DetectTheme(Application.Current);
                switch (value)
                {
                    case 1:
                        ThemeManager.ChangeTheme(Application.Current, theme.Item2, Theme.Dark);
                        Settings.Default.ThemeIndex = 1;
                        break;
                    default:
                        ThemeManager.ChangeTheme(Application.Current, theme.Item2, Theme.Light);
                        Settings.Default.ThemeIndex = 0;
                        break;
                }
            }
        }

        #endregion //Properties

        #region Commands

        public ICommand SaveCommand { get; set; }

        private void Save()
        {
            Settings.Default.Save();
        }

        #endregion //Commands

        #region Event handlers

        private void item_SelectedChanged(object sender, EventArgs e)
        {
            OnSelectedDirectorySearchTypeChanged();
        }

        #endregion // Event handlers

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

        #endregion //Events
    }

    class ResizeTypeDescriptor
    {
        #region Properties

        public string Name { get; private set; }
        public ResizeType Type { get; private set; }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion //Methods

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

        #endregion //Static methods
    }

    class DirectorySearchTypeDescriptor
    {
        #region Fields

        private bool? _isSelected;

        #endregion // Fields

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

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion //Methods

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

        #endregion //Static methods

        #region Events

        public event EventHandler SelectedChanged;
        private void OnSelectedChanged()
        {
            if (SelectedChanged != null)
            {
                SelectedChanged(this, new EventArgs());
            }
        }

        #endregion //Events
    }

    class SortingDescriptor
    {
        #region Properties

        public string Name { get; set; }
        public SortMethod Method { get; set; }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion //Methods

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

        #endregion //Static methods
    }

    class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush ColorBrush { get; set; }

        public void ChangeAccent()
        {
            var theme = ThemeManager.DetectTheme(Application.Current);
            var accent = ThemeManager.DefaultAccents.First(x => x.Name == Name);
            ThemeManager.ChangeTheme(Application.Current, accent, theme.Item1);
            Settings.Default.AccentColorName = Name;
        }
    }

}
