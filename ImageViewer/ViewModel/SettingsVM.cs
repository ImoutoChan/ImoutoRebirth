using ImageViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModel
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
}
