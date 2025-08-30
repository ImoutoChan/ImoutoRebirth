using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal partial class WizardRootVM : ObservableObject
{
    private readonly Window _wizardWindow;

    private readonly ICollectionService _collectionService;
    private readonly IDestinationFolderService _destinationFolderService;
    private readonly ISourceFolderService _sourceFolderService;

    public WizardRootVM(Window wizardWindow)
    {
        _collectionService = ServiceLocator.GetService<ICollectionService>();
        _destinationFolderService = ServiceLocator.GetService<IDestinationFolderService>();
        _sourceFolderService = ServiceLocator.GetService<ISourceFolderService>();

        _wizardWindow = wizardWindow;

        State = new(_wizardWindow);
        State.CreateCollectionRequested += async (_,_) => await CreateAsync();
    }

    [ObservableProperty]
    public partial WizardStateVM State { get; set; }

    [RelayCommand]
    private void Close() => _wizardWindow.Close();

    private async Task CreateAsync()
    {
        var collectionFolder = new DirectoryInfo(State.PathAndName);
        collectionFolder.EnsureExists();

        var createdCollection = await _collectionService.CreateCollectionAsync(State.CollectionName);

        if (State.SelectedThirdStage == WizardStage.ThirdWithDestinationFolder)
        {
            foreach (var sourceFolder in State.SourceFolders)
                await _sourceFolderService.AddSourceFolderAsync(sourceFolder.PrepareToSave(createdCollection.Id));

            var destinationFolder = State.DestinationFolder!;
            await _destinationFolderService.SetDestinationFolderAsync(destinationFolder.PrepareToSave(createdCollection.Id));
        }
        else if (State.SelectedThirdStage == WizardStage.ThirdWithoutDestinationFolder)
        {
            var singleSourceFolder = State.SourceFolders.First();
            await _sourceFolderService.AddSourceFolderAsync(singleSourceFolder.PrepareToSave(createdCollection.Id));
        }

        _wizardWindow.Close();
        Process.Start("explorer", $"\"{collectionFolder.FullName}\"");
    }

    private async Task CreateInDebugModeAsync()
    {
        var collectionFolder = new DirectoryInfo(State.PathAndName);
        await Task.Yield();

        Debug.WriteLine($"[Create] Ensuring directory exists: {State.PathAndName}");

        Debug.WriteLine($"[Create] Creating collection: {State.CollectionName}");
        var fakeCollectionId = Guid.NewGuid(); // симуляция

        if (State.SelectedThirdStage == WizardStage.ThirdWithDestinationFolder)
        {
            Debug.WriteLine("[Create] Selected stage: ThirdWithDestinationFolder");

            foreach (var sourceFolder in State.SourceFolders)
            {
                Debug.WriteLine(
                    $"[Source] Path: {sourceFolder.Path}, " +
                    $"FormatCheck: {sourceFolder.ShouldCheckFormat}, " +
                    $"HashFromName: {sourceFolder.ShouldCheckHashFromName}, " +
                    $"TagsFromSubfolders: {sourceFolder.ShouldCreateTagsFromSubfolders}, " +
                    $"TagFromFilename: {sourceFolder.ShouldAddTagFromFilename}, " +
                    $"Extensions: {string.Join(",", sourceFolder.SupportedExtensions.ToArray() ?? [])}, " +
                    $"WebhookEnabled: {sourceFolder.IsWebhookUploadEnabled}, " +
                    $"WebhookUrl: {sourceFolder.WebhookUploadUrl}"
                );
            }

            Debug.WriteLine(
                $"[Destination] Path: {State.DestinationFolder!.Path}, " +
                $"SubfoldersByHash: {State.DestinationFolder.ShouldCreateSubfoldersByHash}, " +
                $"RenameByHash: {State.DestinationFolder.ShouldRenameByHash}, " +
                $"FormatErrorSubfolder: {State.DestinationFolder!.FormatErrorSubfolder}, " +
                $"HashErrorSubfolder: {State.DestinationFolder.HashErrorSubfolder}, " +
                $"WithoutHashErrorSubfolder: {State.DestinationFolder.WithoutHashErrorSubfolder}"
            );
        }
        else if (State.SelectedThirdStage == WizardStage.ThirdWithoutDestinationFolder)
        {
            Debug.WriteLine("[Create] Selected stage: ThirdWithoutDestinationFolder");

            Debug.WriteLine(
                $"[Source] Path: {State.SourceFolders.First().Path}, " +
                $"FormatCheck: {State.SourceFolders.First().ShouldCheckFormat}, " +
                $"HashFromName: {State.SourceFolders.First().ShouldCheckHashFromName}, " +
                $"TagsFromSubfolders: {State.SourceFolders.First().ShouldCreateTagsFromSubfolders}, " +
                $"TagFromFilename: {State.SourceFolders.First().ShouldAddTagFromFilename}, " +
                $"Extensions: {string.Join(",", State.SourceFolders.First().SupportedExtensions.ToArray() ?? [])}, " +
                $"WebhookEnabled: {State.SourceFolders.First().IsWebhookUploadEnabled}, " +
                $"WebhookUrl: {State.SourceFolders.First().WebhookUploadUrl}"
            );
        }

        Debug.WriteLine("[Create] Would close wizard window.");
        Process.Start("explorer", $"\"{collectionFolder.FullName}\"");
    }
}

public record OpenCreateCollectionWizardRequest;

internal class DesignTimeWizardRootVM : WizardRootVM
{
    public DesignTimeWizardRootVM(Window wizardWindow) : base(wizardWindow)
    {
        State = new WizardStateVM(wizardWindow)
        {
            CollectionName = "Art",
            CollectionPath = "C:\\Path\\To\\Folder",
            WizardStage = WizardStage.FirstNameAndPath
        };
    }

    public DesignTimeWizardRootVM() : base(null!)
    {
        State = new WizardStateVM(null!)
        {
            CollectionName = "Art",
            CollectionPath = "C:\\Path\\To\\Folder",
            WizardStage = WizardStage.FirstNameAndPath
        };
    }
}
