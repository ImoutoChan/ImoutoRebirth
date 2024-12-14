using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice.ValidationAttributes;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardSourceFolderVM : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a path to the folder")]
    [Directory("Please enter valid path to the folder")]
    public partial string Path { get; set; } = "";

    [ObservableProperty]
    public partial bool ShouldCheckHashFromName { get; set; } = false;

    [ObservableProperty]
    public partial bool ShouldCheckFormat { get; set; } = true;

    [ObservableProperty]
    public partial bool ShouldCreateTagsFromSubfolders { get; set; } = false;

    [ObservableProperty]
    public partial bool ShouldAddTagFromFilename { get; set; } = true;

    [ObservableProperty]
    public partial ObservableCollection<string> SupportedExtensions { get; set; } = new();

    public SourceFolder PrepareToSave(Guid collectionId)
    {
        new DirectoryInfo(Path).EnsureExists();
        return new SourceFolder(
            Id: null,
            CollectionId: collectionId,
            Path: Path,
            ShouldCheckFormat: ShouldCheckFormat,
            ShouldCheckHashFromName: ShouldCheckHashFromName,
            ShouldCreateTagsFromSubfolders: ShouldCreateTagsFromSubfolders,
            ShouldAddTagFromFilename: ShouldAddTagFromFilename,
            SupportedExtensions: SupportedExtensions);
    }
}
