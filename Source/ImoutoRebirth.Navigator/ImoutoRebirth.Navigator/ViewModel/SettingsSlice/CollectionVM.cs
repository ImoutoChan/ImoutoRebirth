using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using Serilog;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class CollectionVM : ObservableObject
{
    private readonly ICollectionService _collectionService;
    private readonly IDestinationFolderService _destinationFolderService;
    private readonly ISourceFolderService _sourceFolderService;

    [ObservableProperty]
    public partial Guid Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial SourceFolderVM? SelectedSource { get; set; }

    [ObservableProperty]
    public partial DestinationFolderVM? Destination { get; set; }

    public CollectionVM(Guid id, string name)
    {
        Id = id;
        Name = name;

        _collectionService = ServiceLocator.GetService<ICollectionService>();
        _destinationFolderService = ServiceLocator.GetService<IDestinationFolderService>();
        _sourceFolderService = ServiceLocator.GetService<ISourceFolderService>();
    }

    public ObservableCollection<SourceFolderVM> Sources { get; } = new();

    public async Task LoadFolders()
    {
        try
        {
            var destinationFolder = await _destinationFolderService.GetDestinationFolderAsync(Id);

            Destination = null;

            if (destinationFolder != null)
            {
                var destinationFolderVm = new DestinationFolderVM(
                    destinationFolder.Id,
                    destinationFolder.Path,
                    destinationFolder.ShouldCreateSubfoldersByHash,
                    destinationFolder.ShouldRenameByHash,
                    destinationFolder.FormatErrorSubfolder,
                    destinationFolder.HashErrorSubfolder,
                    destinationFolder.WithoutHashErrorSubfolder);

                destinationFolderVm.ResetRequest += FolderVM_ResetRequest;
                destinationFolderVm.SaveRequest += FolderVM_SaveDestinationRequest;
                destinationFolderVm.RemoveRequest += DestinationFolderVM_RemoveRequest;

                Destination = destinationFolderVm;
            }

            var sourceFolders = await _sourceFolderService.GetSourceFoldersAsync(Id);
            Sources.Clear();

            foreach (var folder in sourceFolders)
            {
                var sourceFolderVm = new SourceFolderVM(
                    folder.Id,
                    folder.Path,
                    folder.ShouldCheckFormat,
                    folder.ShouldCheckHashFromName,
                    folder.SupportedExtensions,
                    folder.ShouldCreateTagsFromSubfolders,
                    folder.ShouldAddTagFromFilename,
                    folder.IsWebhookUploadEnabled,
                    folder.WebhookUploadUrl);
                sourceFolderVm.ResetRequest += FolderVM_ResetRequest;
                sourceFolderVm.SaveRequest += FolderVM_SaveSourceRequest;

                Sources.Add(sourceFolderVm);
            }
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Folders reload error", ex.Message);
            Log.Error(ex, "Folders reload error");
        }
    }
        
    public async Task Remove()
    {
        try
        {
            await _collectionService.DeleteCollectionAsync(Id);
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Can't remove collection", ex.Message);
            Log.Error(ex, "Can't remove collection");
        }
    }

    public async Task Rename(string newName)
    {
        try
        {
            await _collectionService.RenameCollection(Id, newName);

            Name = newName;
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Can't rename collection", ex.Message);
            Log.Error(ex, "Can't rename collection");
        }
    }

    private async void DestinationFolderVM_RemoveRequest(object? sender, EventArgs e)
    {
        if (sender is not FolderVM folderVM)
            return;

        if (folderVM.Id.HasValue)
        {
            try
            {
                await _destinationFolderService.DeleteDestinationFolderAsync(Id);
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't remove folder", ex.Message);
                Log.Error(ex, "Can't remove folder");
            }
        }

        await LoadFolders();
    }

    private async void FolderVM_SaveSourceRequest(object? sender, EventArgs e)
    {
        if (sender is not SourceFolderVM folderVm)
            return;

        var sourceFolder = new SourceFolder(
            folderVm.Id,
            Id,
            folderVm.Path,
            folderVm.CheckFormat,
            folderVm.CheckNameHash,
            folderVm.TagsFromSubfolder,
            folderVm.AddTagFromFileName,
            folderVm.SupportedExtensionsRaw,
            folderVm.IsWebhookUploadEnabled,
            folderVm.WebhookUploadUrl);

        try
        {
            if (sourceFolder.Id.HasValue)
            {
                await _sourceFolderService.UpdateSourceFolderAsync(sourceFolder);
            }
            else
            {
                await _sourceFolderService.AddSourceFolderAsync(sourceFolder);
            }

            await LoadFolders();
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Can't save folder", ex.Message);
            Log.Error(ex, "Can't save folder");
        }
    }

    private async void FolderVM_SaveDestinationRequest(object? sender, EventArgs e)
    {
        if (sender is not DestinationFolderVM folderVm)
            return;

        var destinationFolder = new DestinationFolder(
            folderVm.Id,
            Id,
            folderVm.Path,
            folderVm.NeedDevideImagesByHash,
            folderVm.NeedRename,
            folderVm.IncorrectFormatSubpath ?? "!IncorrectFormat",
            folderVm.IncorrectHashSubpath ?? "!IncorrectHash",
            folderVm.NonHashSubpath ?? "!NonHash");

        try
        {
            await _destinationFolderService.SetDestinationFolderAsync(destinationFolder);
            await LoadFolders();
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Can't save folder", ex.Message);
            Log.Error(ex, "Can't save folder");
        }
    }

    private async void FolderVM_ResetRequest(object? sender, EventArgs e) => await LoadFolders();

    [RelayCommand]
    private void CreateDestinationFolder(object? obj)
    {
        var destinationFolderVM = new DestinationFolderVM(null, string.Empty, false, false, "!IncorrectFormat", "!IncorrectHash", "!NonHash");
        destinationFolderVM.ResetRequest += FolderVM_ResetRequest;
        destinationFolderVM.SaveRequest += FolderVM_SaveDestinationRequest;
        destinationFolderVM.RemoveRequest += DestinationFolderVM_RemoveRequest;
        Destination = destinationFolderVM;
    }

    [RelayCommand]
    private void AddSource(object? param)
    {
        var newSource = new SourceFolderVM(null, string.Empty, false, false, null, false, false, false, null);
        newSource.ResetRequest += FolderVM_ResetRequest;
        newSource.SaveRequest += FolderVM_SaveSourceRequest;
        Sources.Add(newSource);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSource))]
    private async Task RemoveSource(SourceFolderVM? sourceFolderVM)
    {
        if (sourceFolderVM is not FolderVM folderVM)
            return;
        
        if (folderVM.Id.HasValue)
        {
            try
            {
                await _sourceFolderService.DeleteSourceFolderAsync(Id, folderVM.Id.Value);
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Can't remove folder", ex.Message);
                Log.Error(ex, "Can't remove folder");
            }
        }

        await LoadFolders();
    }

    private static bool CanRemoveSource(SourceFolderVM? x) => x != null;
}
