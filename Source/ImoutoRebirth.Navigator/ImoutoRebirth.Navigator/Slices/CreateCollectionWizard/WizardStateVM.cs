using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.WPF.ValidationAttributes;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Slices.CreateCollectionWizard.Steps;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardStateVM : ObservableValidator
{
    private readonly Window _wizardWindow;
    private readonly IMessenger _messenger;

    public WizardStateVM(Window wizardWindow)
    {
        _wizardWindow = wizardWindow;
        _messenger = ServiceLocator.GetService<IMessenger>();

        SourceFolders.CollectionChanged += (_, _) => ValidateAllProperties();
        SourceFolders.CollectionChanged += (_, _) => AdvancedCreateCommand.NotifyCanExecuteChanged();

        PropertyChanged += WizardStateVM_PropertyChanged;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleProgress))]
    public partial WizardStage WizardStage { get; set; } = WizardStage.FirstNameAndPath;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter collection name")]
    [NotifyPropertyChangedFor(nameof(PathAndName), nameof(Title))]
    [NotifyCanExecuteChangedFor(
        nameof(FastCreateCommand),
        nameof(OpenAdvancedCommand),
        nameof(AdvancedCreateCommand))]
    public partial string CollectionName { get; set; } = "New Collection";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotNullOrWhiteSpace("Please enter a path to the folder")]
    [Directory("Please enter valid path to the folder")]
    [NotifyPropertyChangedFor(nameof(PathAndName))]
    [NotifyCanExecuteChangedFor(
        nameof(FastCreateCommand),
        nameof(OpenAdvancedCommand),
        nameof(AdvancedCreateCommand))]
    public partial string CollectionPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    public string PathAndName => Path.Combine(CollectionPath, CollectionName);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdvancedCreateCommand))]
    public partial WizardStage? SelectedThirdStage { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdvancedCreateCommand))]
    public partial ObservableCollection<WizardSourceFolderVM> SourceFolders { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdvancedCreateCommand))]
    public partial WizardDestinationFolderVM? DestinationFolder { get; set; }

    [ObservableProperty]
    public partial UserControl CurrentStageControl { get; set; } = new FastCreateView();

    public string Title => string.IsNullOrWhiteSpace(CollectionName) ? "New collection" : CollectionName;

    public string TitleProgress => WizardStage switch
    {
        WizardStage.FirstNameAndPath => "",
        WizardStage.SecondAdvancedCollectionType => "2 / 3 ",
        _ => "3 / 3 "
    };

    public string TitleCenter => WizardStage switch
    {
        WizardStage.FirstNameAndPath => "",
        WizardStage.SecondAdvancedCollectionType => "",
        WizardStage.ThirdWithDestinationFolder => "Dedicated Folder",
        WizardStage.ThirdWithoutDestinationFolder => "Single Folder",
        _ => ""
    };

    public event EventHandler? CreateCollectionRequested;

    [RelayCommand(CanExecute = nameof(CanFastCreate))]
    private void FastCreate()
    {
        SetFoldersToDefaultWithDestinationSetup();
        SelectedThirdStage = WizardStage.ThirdWithDestinationFolder;

        CreateCollectionRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanFastCreate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

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
    private void SelectDedicatedFolderSetup()
    {
        SetFoldersToDefaultWithDestinationSetup();
        SelectedThirdStage = WizardStage.ThirdWithDestinationFolder;
        WizardStage = WizardStage.ThirdWithDestinationFolder;
    }

    [RelayCommand]
    private void SelectSingleFolderSetup()
    {
        SetFoldersToDefaultWithoutDestinationSetup();
        SelectedThirdStage = WizardStage.ThirdWithoutDestinationFolder;
        WizardStage = WizardStage.ThirdWithoutDestinationFolder;
    }

    private bool CanOpenAdvanced()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

    [RelayCommand(CanExecute = nameof(CanAdvancedCreate))]
    private void AdvancedCreate()
    {
        CreateCollectionRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanAdvancedCreate()
        => !HasErrors
           && SourceFolders.Count > 0
           && SourceFolders.All(x => !x.HasErrors)
           && DestinationFolder is not { HasErrors: true }
           && SelectedThirdStage != null;

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

        if (!string.IsNullOrWhiteSpace(CollectionPath))
        {
            dialog.InitialDirectory = CollectionPath;
        }

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            CollectionPath = dialog.FileName;
        }

        _wizardWindow.Focus();
    }

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
                SupportedExtensions = new(["jpg", "png", "jpeg", "gif", "webm", "swf", "mp4", "zip", "webp"]),
                IsWebhookUploadEnabled = false,
                WebhookUploadUrl = null
            });
        SourceFolders.Add(
            new WizardSourceFolderVM
            {
                Path = Path.Combine(PathAndName, "Source", "WithoutHash"),
                ShouldCheckHashFromName = false,
                ShouldCheckFormat = true,
                ShouldCreateTagsFromSubfolders = false,
                ShouldAddTagFromFilename = false,
                SupportedExtensions = new(["jpg", "png", "jpeg", "gif", "webm", "swf", "mp4", "zip", "webp"]),
                IsWebhookUploadEnabled = false,
                WebhookUploadUrl = null
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
                Path = PathAndName,
                ShouldCheckHashFromName = false,
                ShouldCheckFormat = false,
                ShouldCreateTagsFromSubfolders = true,
                ShouldAddTagFromFilename = true,
                SupportedExtensions = new(),
                IsWebhookUploadEnabled = false,
                WebhookUploadUrl = null
            });

        DestinationFolder = null;
    }

    private void WizardStateVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WizardStage))
        {
            CurrentStageControl = WizardStage switch
            {
                WizardStage.FirstNameAndPath => new FastCreateView(),
                WizardStage.SecondAdvancedCollectionType => new AdvancedView(),
                WizardStage.ThirdWithoutDestinationFolder => new SingleFolderAdvancedView(),
                WizardStage.ThirdWithDestinationFolder => new DedicatedFolderAdvancedView(),
                _ => new FastCreateView()
            };
        }
    }
}

internal enum WizardStage
{
    FirstNameAndPath,
    SecondAdvancedCollectionType,
    ThirdWithDestinationFolder,
    ThirdWithoutDestinationFolder
}
