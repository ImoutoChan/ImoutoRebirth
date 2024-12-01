using System.IO;
using System.Security.Cryptography;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class FileInfoVM : ObservableObject
{
    private readonly IFileService _fileService;

    private FileInfo? _fileInfo;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private long? _size;

    [ObservableProperty]
    private Size? _pixelSize;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalculateHashCommand))]
    [NotifyPropertyChangedFor(nameof(IsHashValid))]
    private string? _hash;

    [ObservableProperty]
    private int _orderNumber;

    [ObservableProperty]
    private bool _hasValue;

    [ObservableProperty]
    private DateTimeOffset? _addedOn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHashValid))]
    private string? _storedHash;

    public bool IsHashValid => Hash == null || StoredHash == null || Hash == StoredHash;

    public FileInfoVM()
    {
        _fileService = ServiceLocator.GetService<IFileService>();
    }

    private bool CanCalculateHash() => Hash == null;
    
    [RelayCommand(CanExecute = nameof(CanCalculateHash))]
    private async Task CalculateHashAsync()
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

    public async void UpdateCurrentInfo(INavigatorListEntry? navigatorListEntry, int number)
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

        if (navigatorListEntry.DbId.HasValue)
        {
            var metadata = await _fileService.GetFileMetadata(navigatorListEntry.DbId.Value);
            AddedOn = metadata.AddedOn;
            StoredHash = metadata.StoredMd5;
        }
    }
}
