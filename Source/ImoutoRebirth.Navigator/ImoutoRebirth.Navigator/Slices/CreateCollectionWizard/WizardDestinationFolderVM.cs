using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice.ValidationAttributes;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardDestinationFolderVM : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a path to the folder")]
    [Directory("Please enter valid path to the folder")]
    public partial string Path { get; set; } = "";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace]
    [Directory("Please enter valid folder name")]
    public partial bool ShouldCreateSubfoldersByHash { get; set; } = false;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter folder name")]
    [Directory("Please enter valid folder name")]
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
}
