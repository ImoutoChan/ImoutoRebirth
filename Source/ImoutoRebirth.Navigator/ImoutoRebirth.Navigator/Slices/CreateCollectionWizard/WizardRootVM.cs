using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;

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

        _wizardWindow.Close();
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
}
