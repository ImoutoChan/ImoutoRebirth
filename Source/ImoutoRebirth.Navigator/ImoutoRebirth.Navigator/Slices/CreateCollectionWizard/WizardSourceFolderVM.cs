using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;

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

    [ObservableProperty]
    public partial bool IsWebhookUploadEnabled { get; set; } = false;

    [ObservableProperty]
    public partial string? WebhookUploadUrl { get; set; }

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
            SupportedExtensions: SupportedExtensions,
            IsWebhookUploadEnabled: IsWebhookUploadEnabled,
            WebhookUploadUrl: WebhookUploadUrl);
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
