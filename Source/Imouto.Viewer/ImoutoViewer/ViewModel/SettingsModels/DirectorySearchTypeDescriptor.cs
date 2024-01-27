using System.Collections.ObjectModel;
using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel.SettingsModels;

internal class DirectorySearchTypeDescriptor
{
    #region Fields

    private bool? _isSelected;

    #endregion  Fields

    #region Properties

    public required string Name { get; init; }
    public required DirectorySearchFlags Type { get; init; }
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
            new() { Name = "All Pre", Type = DirectorySearchFlags.AllDepthPrefolder },
            new() { Name = "Pre", Type = DirectorySearchFlags.Prefolders },
            new() { Name = "Cur", Type = DirectorySearchFlags.Folder },
            new() { Name = "Sub", Type = DirectorySearchFlags.Subfolders },
            new() { Name = "All Sub", Type = DirectorySearchFlags.AllDepthSubfolders },
        };
    }

    #endregion Static methods

    #region Events

    public event EventHandler? SelectedChanged;
    private void OnSelectedChanged() => SelectedChanged?.Invoke(this, EventArgs.Empty);

    #endregion Events
}
