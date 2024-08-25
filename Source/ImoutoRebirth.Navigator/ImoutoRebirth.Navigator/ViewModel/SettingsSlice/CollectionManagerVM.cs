using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class CollectionManagerVm : ObservableObject
{
    private readonly ICollectionService _collectionService;

    [ObservableProperty]
    private CollectionVM? _selectedCollection;

    public CollectionManagerVm() => _collectionService = ServiceLocator.GetService<ICollectionService>();

    public ObservableCollection<CollectionVM> Collections { get; } = new();

    public string? Rename(string param)
    {
        try
        {
            SelectedCollection?.Rename(param);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task<string?> CreateCollection(string name)
    {
        try
        {
            await _collectionService.CreateCollectionAsync(name);
            await ReloadCollectionsAsync();

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Cannot create collection: " + ex.Message);
            return ex.Message;
        }
    }

    public async Task ReloadCollectionsAsync()
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            var collections = await _collectionService.GetAllCollectionsAsync();

            sw.Stop();
            Debug.WriteLine($"Collections reloaded in {sw.ElapsedMilliseconds}ms.");

            Collections.Clear();

            foreach (var collection in collections)
            {
                Collections.Add(new CollectionVM(collection.Id, collection.Name));
            }

            foreach (var collectionVm in Collections)
            {
                await collectionVm.LoadFolders();
            }

            SelectedCollection = Collections.FirstOrDefault();
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Can't reload collections", ex.Message);
            Debug.WriteLine("Collections reload error: " + ex.Message);
        }
    }


    [RelayCommand(CanExecute = nameof(CanDoCollectionCommand))]
    private async Task Remove()
    {
        if (SelectedCollection != null)
        {
            await SelectedCollection.Remove();
            await ReloadCollectionsAsync();
        }
    }

    private bool CanDoCollectionCommand() => SelectedCollection != null;
}
