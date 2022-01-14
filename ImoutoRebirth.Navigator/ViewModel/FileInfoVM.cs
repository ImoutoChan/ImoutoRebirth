using System.IO;
using System.Windows.Input;
using Imouto.Utils.Core;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

class FileInfoVM : VMBase
{
    #region Fields

    private string _name;
    private string _hash;
    private long? _size;
    private int _orderNumber;
    private bool _hasValue;
    private ICommand _calculateHashCommand;
    private FileInfo _fileInfo;

    #endregion Fields

    #region Properties

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            OnPropertyChanged(ref _name, value, () => Name);
        }
    }

    public long? Size
    {
        get
        {
            return _size;
        }
        set
        {
            OnPropertyChanged(ref _size, value, () => Size);
        }
    }

    public string Hash
    {
        get
        {
            return _hash;
        }
        set
        {
            OnPropertyChanged(ref _hash, value, () => Hash);
        }
    }

    public int OrderNumber
    {
        get
        {
            return _orderNumber;
        }
        set
        {
            OnPropertyChanged(ref _orderNumber, value, () => OrderNumber);
        }
    }

    public bool HasValue
    {
        get
        {
            return _hasValue;
        }
        set
        {
            OnPropertyChanged(ref _hasValue, value, () => HasValue);
        }
    }

    #endregion Properties

    #region Commands

    public ICommand CalculateHashCommand
    {
        get
        {
            return _calculateHashCommand ??
                   (_calculateHashCommand = new RelayCommand((s) => CalculateHash(), (s) => Hash == null));
        }
    }

    #endregion Commands

    #region Methods

    private async void CalculateHash()
    {
        Hash = "Calculating...";

        Hash = await _fileInfo.GetMd5ChecksumAsync();
    }

    public void UpdateCurrentInfo(INavigatorListEntry navigatorListEntry, int number)
    {
        OrderNumber = number;

        if (navigatorListEntry == null)
        {
            HasValue = false;
            Name = null;
            Size = null;
            Hash = null;
            _fileInfo = null;
            return;
        }

        var fi = new FileInfo(navigatorListEntry.Path);
        if (!fi.Exists)
        {
            return;
        }

        HasValue = true;
        Name = fi.Name;
        Size = fi.Length;
        Hash = null;
        _fileInfo = fi;
    }

    #endregion Methods
}