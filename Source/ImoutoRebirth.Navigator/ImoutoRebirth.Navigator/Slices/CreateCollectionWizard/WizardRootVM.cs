using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;
using Serilog;

namespace ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;

internal record CreateCollectionRequested;

internal partial class WizardRootVM : ObservableObject, IRecipient<CreateCollectionRequested>
{
    private readonly IMessenger _messenger;

    private readonly ICollectionService _collectionService;
    private readonly IDestinationFolderService _destinationFolderService;
    private readonly ISourceFolderService _sourceFolderService;

    public WizardRootVM()
    {
        _collectionService = ServiceLocator.GetService<ICollectionService>();
        _destinationFolderService = ServiceLocator.GetService<IDestinationFolderService>();
        _sourceFolderService = ServiceLocator.GetService<ISourceFolderService>();

        _messenger = ServiceLocator.GetService<IMessenger>();
        _messenger.Register(this);
    }

    [ObservableProperty]
    public partial WizardStateVM State { get; set; } = new();

    public async void Receive(CreateCollectionRequested message)
    {
        try
        {
            await CreateAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to create collection");
        }
    }

    private async Task CreateAsync()
    {
        new DirectoryInfo(State.PathAndName).EnsureExists();

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
    }
}

internal class DesignTimeWizardRootVM : WizardRootVM
{
    public DesignTimeWizardRootVM()
    {
        State = new WizardStateVM
        {
            CollectionName = "Art",
            CollectionPath = "C:\\Path\\To\\Folder",
            WizardStage = WizardStage.FirstNameAndPath
        };
    }
}
