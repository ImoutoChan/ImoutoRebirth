using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice.ValidationAttributes;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardStateVM : ObservableValidator
{
    private readonly IMessenger _messenger;

    public WizardStateVM()
    {
        _messenger = ServiceLocator.GetService<IMessenger>();

        SourceFolders.CollectionChanged += (_, _) => ValidateAllProperties();
        SourceFolders.CollectionChanged += (_, _) => AdvancedCreateCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    public partial WizardStage WizardStage { get; set; } = WizardStage.FirstNameAndPath;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter collection name")]
    [NotifyPropertyChangedFor(nameof(PathAndName))]
    [NotifyCanExecuteChangedFor(nameof(FastCreateCommand), nameof(OpenAdvancedCommand), nameof(ConfigureFoldersCommand), nameof(AdvancedCreateCommand))]
    public partial string CollectionName { get; set; } = "";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a path to the folder")]
    [Directory("Please enter valid path to the folder")]
    [NotifyPropertyChangedFor(nameof(PathAndName))]
    [NotifyCanExecuteChangedFor(nameof(FastCreateCommand), nameof(OpenAdvancedCommand), nameof(ConfigureFoldersCommand), nameof(AdvancedCreateCommand))]
    public partial string CollectionPath { get; set; } = "";

    public string PathAndName => Path.Combine(CollectionPath, CollectionName);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfigureFoldersCommand), nameof(AdvancedCreateCommand))]
    public partial WizardStage? SelectedThirdStage { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdvancedCreateCommand))]
    public partial ObservableCollection<WizardSourceFolderVM> SourceFolders { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdvancedCreateCommand))]
    public partial WizardDestinationFolderVM? DestinationFolder { get; set; }

    [RelayCommand(CanExecute = nameof(CanFastCreate))]
    private void FastCreate()
    {
        SetFoldersToDefaultWithDestinationSetup();
        SelectedThirdStage = WizardStage.ThirdWithDestinationFolder;

        _messenger.Send(new CreateCollectionRequested());
    }

    private bool CanFastCreate() => !HasErrors;

    [RelayCommand(CanExecute = nameof(CanOpenAdvanced))]
    private void OpenAdvanced()
    {
        SelectedThirdStage = null;
        WizardStage = WizardStage.SecondAdvancedCollectionType;
    }

    [RelayCommand]
    private void Back()
    {
        WizardStage 
            = WizardStage == WizardStage.SecondAdvancedCollectionType 
                ? WizardStage.FirstNameAndPath 
                : WizardStage.SecondAdvancedCollectionType;
    }

    [RelayCommand]
    private void SelectDedicatedFolderSetup() => WizardStage = WizardStage.ThirdWithDestinationFolder;

    [RelayCommand]
    private void SelectOneFolderSetup() => WizardStage = WizardStage.ThirdWithoutDestinationFolder;

    private bool CanOpenAdvanced() => !HasErrors;

    [RelayCommand(CanExecute = nameof(CanConfigureFolders))]
    private void ConfigureFolders()
    {
        if (SelectedThirdStage == WizardStage.ThirdWithDestinationFolder)
        {
            SetFoldersToDefaultWithDestinationSetup();
            WizardStage = WizardStage.ThirdWithDestinationFolder;
        }
        else
        {
            SetFoldersToDefaultWithoutDestinationSetup();
            WizardStage = WizardStage.ThirdWithoutDestinationFolder;
        }
    }

    private bool CanConfigureFolders() => !HasErrors && SelectedThirdStage != null;

    [RelayCommand(CanExecute = nameof(CanAdvancedCreate))]
    private void AdvancedCreate() => _messenger.Send(new CreateCollectionRequested());

    private bool CanAdvancedCreate()
        => !HasErrors
           && SourceFolders.Count > 0
           && SourceFolders.All(x => !x.HasErrors)
           && DestinationFolder is not { HasErrors: true }
           && SelectedThirdStage != null;

    private void SetFoldersToDefaultWithDestinationSetup()
    {
        SourceFolders.Clear();
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = Path.Combine(PathAndName, "Source", "WithHash"),
                ShouldCheckHashFromName = true,
                ShouldCheckFormat = true,
                ShouldCreateTagsFromSubfolders = false,
                ShouldAddTagFromFilename = false,
                SupportedExtensions = ["jpg", "png", "jpeg", "gif"]
            });
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = Path.Combine(PathAndName, "Source", "WithHash"),
                ShouldCheckHashFromName = true,
                ShouldCheckFormat = false,
                ShouldCreateTagsFromSubfolders = false,
                ShouldAddTagFromFilename = false,
                SupportedExtensions = ["webm", "swf", "mp4", "zip", "webp"]
            });
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = Path.Combine(PathAndName, "Source", "WithoutHash"),
                ShouldCheckHashFromName = false,
                ShouldCheckFormat = true,
                ShouldCreateTagsFromSubfolders = false,
                ShouldAddTagFromFilename = false,
                SupportedExtensions = ["jpg", "png", "jpeg", "gif"]
            });
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = Path.Combine(PathAndName, "Source", "WithoutHash"),
                ShouldCheckHashFromName = false,
                ShouldCheckFormat = false,
                ShouldCreateTagsFromSubfolders = false,
                ShouldAddTagFromFilename = false,
                SupportedExtensions = ["webm", "swf", "mp4", "zip", "webp"]
            });

        DestinationFolder = new WizardDestinationFolderVM
        {
            Path = Path.Combine(PathAndName, "Collection"),
            ShouldCreateSubfoldersByHash = true,
            ShouldRenameByHash = true,
            FormatErrorSubfolder = "!InvalidFormat",
            HashErrorSubfolder = "!InvalidHash",
            WithoutHashErrorSubfolder = "!WithoutHash"
        };
    }

    private void SetFoldersToDefaultWithoutDestinationSetup()
    {
        SourceFolders.Clear();
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = "",
                ShouldCheckHashFromName = false,
                ShouldCheckFormat = false,
                ShouldCreateTagsFromSubfolders = true,
                ShouldAddTagFromFilename = true,
                SupportedExtensions = []
            });

        DestinationFolder = null;
    }
}

internal enum WizardStage
{
    FirstNameAndPath,
    SecondAdvancedCollectionType,
    ThirdWithDestinationFolder,
    ThirdWithoutDestinationFolder
}
