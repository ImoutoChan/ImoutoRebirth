using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class FileInfoVM : VMBase
{
    private string? _name;
    private string? _hash;
    private long? _size;
    private Size? _pixelSize;
    private int _orderNumber;
    private bool _hasValue;
    private ICommand? _calculateHashCommand;
    private FileInfo? _fileInfo;

    public string? Name
    {
        get => _name;
        set => OnPropertyChanged(ref _name, value, () => Name);
    }

    public long? Size
    {
        get => _size;
        set => OnPropertyChanged(ref _size, value, () => Size);
    }

    public Size? PixelSize
    {
        get => _pixelSize;
        set => OnPropertyChanged(ref _pixelSize, value, () => PixelSize);
    }

    public string? Hash
    {
        get => _hash;
        set => OnPropertyChanged(ref _hash, value, () => Hash);
    }

    public int OrderNumber
    {
        get => _orderNumber;
        set => OnPropertyChanged(ref _orderNumber, value, () => OrderNumber);
    }

    public bool HasValue
    {
        get => _hasValue;
        set => OnPropertyChanged(ref _hasValue, value, () => HasValue);
    }

    public ICommand CalculateHashCommand 
        => _calculateHashCommand ??= new RelayCommand(_ => CalculateHash(), _ => Hash == null);

    private async void CalculateHash()
    {
        Hash = "Calculating...";

        Hash = await Task.Run(() =>
        {
            if (_fileInfo?.Exists != true)
                throw new ArgumentException("File does not exist.");

            using var md5 = MD5.Create();
            using var stream = _fileInfo.OpenRead();
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
        });
    }

    public void UpdateCurrentInfo(INavigatorListEntry? navigatorListEntry, int number)
    {
        OrderNumber = number;

        if (navigatorListEntry == null)
        {
            HasValue = false;
            Name = null;
            Size = null;
            Hash = null;
            PixelSize = null;
            _fileInfo = null;
            return;
        }

        var fi = new FileInfo(navigatorListEntry.Path);
        if (!fi.Exists)
            return;

        HasValue = true;
        Name = fi.Name;
        Size = fi.Length;
        Hash = null;
        PixelSize = navigatorListEntry is IPixelSizable pixelSizable ? pixelSizable.PixelSize : null;
        _fileInfo = fi;
    }
}
