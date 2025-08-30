using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardDestinationFolderVM : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a path to the folder")]
    [Directory("Please enter valid path to the folder")]
    public partial string Path { get; set; } = "";

    [ObservableProperty]
    public partial bool ShouldCreateSubfoldersByHash { get; set; } = false;

    [ObservableProperty]
    public partial bool ShouldRenameByHash { get; set; } = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
    public partial string FormatErrorSubfolder { get; set; } = "!InvalidFormat";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
    public partial string HashErrorSubfolder { get; set; } = "!InvalidHash";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter a valid folder name")]
    public partial string WithoutHashErrorSubfolder { get; set; } = "!WithoutHash";

    public DestinationFolder PrepareToSave(Guid collectionId)
    {
        new DirectoryInfo(Path).EnsureExists();
        return new DestinationFolder(
            Id: null,
            CollectionId: collectionId,
            Path: Path,
            ShouldCreateSubfoldersByHash: ShouldCreateSubfoldersByHash,
            ShouldRenameByHash: ShouldRenameByHash,
            FormatErrorSubfolder: FormatErrorSubfolder,
            HashErrorSubfolder: HashErrorSubfolder,
            WithoutHashErrorSubfolder: WithoutHashErrorSubfolder);
    }

    [RelayCommand]
    private void BrowsePathLocation()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = "Select a folder",
            AllowNonFileSystemItems = false,
            Multiselect = false,
            EnsurePathExists = true,
            NavigateToShortcut = true
        };

        if (!string.IsNullOrWhiteSpace(Path))
        {
            dialog.InitialDirectory = Path;
        }

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            Path = dialog.FileName;
        }
    }
}
