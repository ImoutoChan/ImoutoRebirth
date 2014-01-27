using ImoutoViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ImoutoViewer.ViewModel
{
    class SettingsVM : VMBase
    {
        #region Fields

        private ResizeTypeDescriptor _selectedResizeType;

        #endregion //Fileds
        
        #region Constructors

        public SettingsVM()
        {
            ResizeTypes = ResizeTypeDescriptor.GetList();
            var selected =
                from s in ResizeTypes
                where s.Type == ResizeType.DownscaleToViewPort
                select s;
            SelectedResizeType = selected.First();

            DirectorySearchTypes = DirectorySearchTypeDescriptor.GetList();
            foreach (var item in DirectorySearchTypes)
            {
                item.SelectedChanged += item_SelectedChanged;
            }
        }

        void item_SelectedChanged(object sender, EventArgs e)
        {
            OnSelectedDirectorySearchTypeChanged();
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
                FilesGettingMethod fg = FilesGettingMethod.None;
                foreach (var item in DirectorySearchTypes)
                {
                    if (item.IsSelected)
                    {
                        fg |= item.Type;
                    }
                }
                return fg;
            }
        }

        #endregion //Properties

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
            return new List<ResizeTypeDescriptor>()
            {
                new ResizeTypeDescriptor() { Name = "Fit to screen (downscale only)", Type = ResizeType.DownscaleToViewPort },
                new ResizeTypeDescriptor() { Name = "Fit to screen (down & up scale)", Type = ResizeType.FitToViewPort },            
                new ResizeTypeDescriptor() { Name = "Fit to screen width (downscale only)", Type = ResizeType.DownscaleToViewPortWidth },            
                new ResizeTypeDescriptor() { Name = "Fit to screen width (down & up scale)", Type = ResizeType.FitToViewPortWidth },            
                new ResizeTypeDescriptor() { Name = "Original size (no resize)", Type = ResizeType.NoResize },
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
            return new ObservableCollection<DirectorySearchTypeDescriptor>()
            {
                new DirectorySearchTypeDescriptor() { Name = "All Pre", Type = FilesGettingMethod.AllDepthPrefolder },
                new DirectorySearchTypeDescriptor() { Name = "Pre", Type = FilesGettingMethod.Prefolders },            
                new DirectorySearchTypeDescriptor() { Name = "Cur", Type = FilesGettingMethod.Folder, IsSelected = true },            
                new DirectorySearchTypeDescriptor() { Name = "Sub", Type = FilesGettingMethod.Subfolders, IsSelected = true},            
                new DirectorySearchTypeDescriptor() { Name = "All Sub", Type = FilesGettingMethod.AllDepthSubfolders },
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
}
