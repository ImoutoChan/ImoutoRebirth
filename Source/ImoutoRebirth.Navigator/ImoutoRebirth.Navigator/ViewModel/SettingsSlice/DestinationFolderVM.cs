using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common.WPF.ValidationAttributes;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class DestinationFolderVM : FolderVM
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    [Directory("Please enter valid folder name")]
    private bool _needDevideImagesByHash;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
    private bool _needRename;
    
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
    private string? _incorrectHashSubpath;
    
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
        _needDevideImagesByHash = needDevideImagesByHash;
        _needRename = needRename;
        _incorrectFormatSubpath = incorrectFormatSubpath;
        _incorrectHashSubpath = incorrectHashSubpath;
        _nonHashSubpath = nonHashSubpath;
    }

    [RelayCommand]
    private void Remove() => RemoveRequest?.Invoke(this, EventArgs.Empty);

    public event EventHandler? RemoveRequest;
}
