using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class CollectionManagerVm : VMBase
{
    private readonly ICollectionService _collectionService;

    private ICommand? _removeCommand;
    private CollectionVM? _selectedCollection;

    public CollectionManagerVm() => _collectionService = ServiceLocator.GetService<ICollectionService>();

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

    #region Properties

    public ObservableCollection<CollectionVM> Collections { get; } = new ObservableCollection<CollectionVM>();

    public CollectionVM? SelectedCollection
    {
        get => _selectedCollection;
        set => OnPropertyChanged(ref _selectedCollection, value, () => this.SelectedCollection);
    }

    #endregion Properties

    #region Methods
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

    #endregion Methods

    #region Commands

    public ICommand RemoveCommand
        => _removeCommand ??= new AsyncCommand(Remove, CanDoCollectionCommand);

    #endregion Commands

    #region Command Handlers


    private async Task Remove()
    {
        if (SelectedCollection != null)
        {
            await SelectedCollection.Remove();
            await ReloadCollectionsAsync();
        }
    }

    private bool CanDoCollectionCommand()
    {
        return SelectedCollection != null;
    }

    #endregion Command Handlers
}
