using System.Collections.ObjectModel;
using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel.SettingsModels;

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
                _isSelected = ((Type & Properties.Settings.Default.DirectorySearchFlags) == Type);
            }
            return (bool)_isSelected;
        }
        set
        {
            _isSelected = value;
            Properties.Settings.Default.DirectorySearchFlags = (value)
                ? Properties.Settings.Default.DirectorySearchFlags | Type
                : Properties.Settings.Default.DirectorySearchFlags & ~Type;
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