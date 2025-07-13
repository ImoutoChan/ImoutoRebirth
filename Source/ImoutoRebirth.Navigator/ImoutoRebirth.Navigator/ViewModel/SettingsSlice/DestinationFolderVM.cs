using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common.WPF.ValidationAttributes;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class DestinationFolderVM : FolderVM
{
    [ObservableProperty]
    public partial bool NeedDevideImagesByHash { get; set; }

    [ObservableProperty]
    public partial bool NeedRename { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
    private string? _incorrectFormatSubpath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
    public partial string? IncorrectHashSubpath { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter a valid folder name")]
    private string? _nonHashSubpath;

    public DestinationFolderVM(
        Guid? id,
        string path,
        bool needDevideImagesByHash,
        bool needRename,
        string incorrectFormatSubpath,
        string incorrectHashSubpath,
        string nonHashSubpath)
        : base(id, path)
    {
        NeedDevideImagesByHash = needDevideImagesByHash;
        NeedRename = needRename;
        _incorrectFormatSubpath = incorrectFormatSubpath;
        IncorrectHashSubpath = incorrectHashSubpath;
        _nonHashSubpath = nonHashSubpath;
    }

    [RelayCommand]
    private void Remove() => RemoveRequest?.Invoke(this, EventArgs.Empty);

    public event EventHandler? RemoveRequest;
}
