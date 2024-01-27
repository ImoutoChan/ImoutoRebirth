using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class CollectionVM : VMBase
{
    private ICommand? _addSourceCommand;
    private ICommand? _removeSourceCommand;
    private ICommand? _createDestinationFolderCommand;

    private Guid _id;
    private string _name;
    private DestinationFolderVM? _destination;
    private SourceFolderVM? _selectedSource;
    private readonly ICollectionService _collectionService;
    private readonly IDestinationFolderService _destinationFolderService;
    private readonly ISourceFolderService _sourceFolderService;

    public CollectionVM(Guid id, string name)
    {
        _id = id;
        _name = name;

        _collectionService = ServiceLocator.GetService<ICollectionService>();
        _destinationFolderService = ServiceLocator.GetService<IDestinationFolderService>();
        _sourceFolderService = ServiceLocator.GetService<ISourceFolderService>();
    }

    #region Properties

    public Guid Id
    {
        get => _id;
        set => OnPropertyChanged(ref _id, value, () => Id);
    }

    public string Name
    {
        get => _name;
        set => OnPropertyChanged(ref _name, value, () => Name);
    }

    public ObservableCollection<SourceFolderVM> Sources { get; } = new();

    public SourceFolderVM? SelectedSource
    {
        get => _selectedSource;
        set { OnPropertyChanged(ref _selectedSource, value, () => SelectedSource); }
    }

    public DestinationFolderVM? Destination
    {
        get => _destination;
        set { OnPropertyChanged(ref _destination, value, () => Destination); }
    }

    #endregion Properties

    #region Methods

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
                    folder.ShouldAddTagFromFilename);
                sourceFolderVm.ResetRequest += FolderVM_ResetRequest;
                sourceFolderVm.SaveRequest += FolderVM_SaveSourceRequest;

                Sources.Add(sourceFolderVm);
            }
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Folders reload error", ex.Message);
            Debug.WriteLine("Folders reload error: " + ex.Message);
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
            Debug.WriteLine("Can't remove collection: " + ex.Message);
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
            Debug.WriteLine("Can't rename collection: " + ex.Message);
        }
    }

    #endregion Methods

    #region Handlers
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
                Debug.WriteLine("Can't remove folder: " + ex.Message);
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
            folderVm.SupportedExtensionsRaw);

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
            Debug.WriteLine("Can't save folder: " + ex.Message);
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
            Debug.WriteLine("Can't save folder: " + ex.Message);
        }
    }

    private async void FolderVM_ResetRequest(object? sender, EventArgs e) => await LoadFolders();

    #endregion Handlers

    #region Commands

    public ICommand AddSourceCommand => _addSourceCommand ??= new RelayCommand(AddSource);

    public ICommand RemoveSourceCommand => _removeSourceCommand ??= new AsyncCommand(RemoveSource, CanRemoveSource);
    public ICommand CreateDestinationFolderCommand 
        => _createDestinationFolderCommand ??= new RelayCommand(CreateDestinationFolder);

    private void CreateDestinationFolder(object? obj)
    {
        var destinationFolderVM = new DestinationFolderVM(null, string.Empty, false, false, "!IncorrectFormat", "!IncorrectHash", "!NonHash");
        destinationFolderVM.ResetRequest += FolderVM_ResetRequest;
        destinationFolderVM.SaveRequest += FolderVM_SaveDestinationRequest;
        destinationFolderVM.RemoveRequest += DestinationFolderVM_RemoveRequest;
        Destination = destinationFolderVM;
    }

    #endregion Commands

    #region Command Handlers

    private void AddSource(object? param)
    {
        var newSource = new SourceFolderVM(null, String.Empty, false, false, null, false, false);
        newSource.ResetRequest += FolderVM_ResetRequest;
        newSource.SaveRequest += FolderVM_SaveSourceRequest;
        Sources.Add(newSource);
    }

    private async Task RemoveSource()
    {
        if (SelectedSource != null)
        { 
            var folderVM = SelectedSource as FolderVM;

            if (folderVM.Id.HasValue)
            {
                try
                {
                    await _sourceFolderService.DeleteSourceFolderAsync(Id, folderVM.Id.Value);
                }
                catch (Exception ex)
                {
                    App.MainWindowVM?.SetStatusError("Can't remove folder", ex.Message);
                    Debug.WriteLine("Can't remove folder: " + ex.Message);
                }
            }

            await LoadFolders();
        }
    }


    public bool CanRemoveSource()
    {
        return SelectedSource != null;
    }

    #endregion Command Handlers
}
