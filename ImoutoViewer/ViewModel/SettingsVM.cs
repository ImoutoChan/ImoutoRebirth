using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoViewer.Commands;
using ImoutoViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MahApps.Metro;

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

        #endregion //Fileds
        
        #region Constructors

        public SettingsVM()
        {
            ResizeTypes = ResizeTypeDescriptor.GetList();
            SelectedResizeType = ResizeTypes.First(x => x.Type == ResizeType.DownscaleToViewPort);

            DirectorySearchTypes = DirectorySearchTypeDescriptor.GetList();
            foreach (var item in DirectorySearchTypes)
            {
                item.SelectedChanged += item_SelectedChanged;
            }

            SortingMethods = SortingDescriptor.GetList();

            SelectedFoldersSorting = SortingMethods.First(x => x.Method == SortMethod.ByName);
            SelectedFilesSorting = SortingMethods.First(x => x.Method == SortMethod.ByName);

            IsSelectedFilesSortingDescending = false;
            IsSelectedFoldersSortingDescending = false;

            AccentColors = ThemeManager.DefaultAccents
                                .Select(a => new AccentColorMenuData
                                {
                                    Name = a.Name, 
                                    ColorBrush = a.Resources["AccentColorBrush"] as Brush
                                })
                                .ToList();

            var accent = ThemeManager.DetectTheme(Application.Current);
            SelectedAccentColor = AccentColors.First(x => x.Name == accent.Item2.Name);
        }

        #endregion //Constructors

        #region Properties

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
                OnSelectedResizeTypeChanged();
            }
        }

        public ObservableCollection<DirectorySearchTypeDescriptor> DirectorySearchTypes { get; private set; }
        public FilesGettingMethod DirectorySearchFlags
        {
            get
            {
                return DirectorySearchTypes.Where(item => item.IsSelected).Aggregate(FilesGettingMethod.None, (current, item) => current | item.Type);
            }
        }

        public List<SortingDescriptor> SortingMethods { get; set; }

        public SortingDescriptor SelectedFoldersSorting
        {
            get { return _selectedFoldersSorting; }
            set
            {
                _selectedFoldersSorting = value;
                OnSelectedFoldersSortingChanged();
            }
        }

        public bool IsSelectedFoldersSortingDescending
        {
            get { return _isSelectedFoldersSortingDescending; }
            set
            {
                _isSelectedFoldersSortingDescending = value;
                OnSelectedFoldersSortingChanged();
            }
        }

        public SortingDescriptor SelectedFilesSorting
        {
            get { return _selectedFilesSorting; }
            set
            {
                _selectedFilesSorting = value;
                OnSelectedFilesSortingChanged();
            }
        }

        public bool IsSelectedFilesSortingDescending
        {
            get { return _isSelectedFilesSortingDescending; }
            set
            {
                _isSelectedFilesSortingDescending = value;
                OnSelectedFilesSortingChanged();
            }
        }

        public List<AccentColorMenuData> AccentColors { get; set; }

        public AccentColorMenuData SelectedAccentColor
        {
            get
            {
                return _selectedAccentColor;
            }
            set
            {
                _selectedAccentColor = value;
                _selectedAccentColor.ChangeAccent(this);
            }
        }

        #endregion //Properties

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

        public string Name { get; set; }
        public ResizeType Type { get; set; }

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

        private bool _isSelected;

        #endregion // Fields

        #region Properties

        public string Name { get; set; }
        public FilesGettingMethod Type { get; set; }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
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
                new DirectorySearchTypeDescriptor { Name = "All Pre", Type = FilesGettingMethod.AllDepthPrefolder },
                new DirectorySearchTypeDescriptor { Name = "Pre", Type = FilesGettingMethod.Prefolders },            
                new DirectorySearchTypeDescriptor { Name = "Cur", Type = FilesGettingMethod.Folder, IsSelected = true },            
                new DirectorySearchTypeDescriptor { Name = "Sub", Type = FilesGettingMethod.Subfolders, IsSelected = true},            
                new DirectorySearchTypeDescriptor { Name = "All Sub", Type = FilesGettingMethod.AllDepthSubfolders },
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

        public static List<SortingDescriptor> GetList()
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

        public void ChangeAccent(object sender)
        {
            var theme = ThemeManager.DetectTheme(Application.Current);
            var accent = ThemeManager.DefaultAccents.First(x => x.Name == Name);
            ThemeManager.ChangeTheme(Application.Current, accent, theme.Item1);
        }
    }

}
