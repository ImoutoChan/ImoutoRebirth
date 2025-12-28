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
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial long? Size { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PixelSizeWidth), nameof(PixelSizeHeight))]
    private partial Size? PixelSize { get; set; }

    public double? PixelSizeWidth => PixelSize?.Width;

    public double? PixelSizeHeight => PixelSize?.Height;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CalculateHashCommand))]
    [NotifyPropertyChangedFor(nameof(IsHashValid))]
    public partial string? Hash { get; set; }

    [ObservableProperty]
    public partial int OrderNumber { get; set; }

    [ObservableProperty]
    public partial bool HasValue { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? AddedOn { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHashValid))]
    public partial string? StoredHash { get; set; }

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

        if (navigatorListEntry is IPixelSizable pixelSizable)
        {
            PixelSize = pixelSizable.PixelSize;

            if (navigatorListEntry is ObservableObject oo)
            {
                oo.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == "PixelSize")
                    {
                        PixelSize = pixelSizable.PixelSize;
                    }
                };
            }
        }
        else
        {
            PixelSize = null;
        }

        _fileInfo = fi;

        if (navigatorListEntry.DbId.HasValue)
        {
            var metadata = await _fileService.GetFileMetadata(navigatorListEntry.DbId.Value);
            AddedOn = metadata.AddedOn;
            StoredHash = metadata.StoredMd5;
        }
    }
}
