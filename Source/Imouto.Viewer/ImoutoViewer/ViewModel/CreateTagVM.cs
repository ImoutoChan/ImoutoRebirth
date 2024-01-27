using System.Collections.ObjectModel;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ViewModel;

internal class CreateTagVM : VMBase
{
    private static readonly SemaphoreSlim ReloadTagTypesAsyncSemaphore = new(1, 1);

    private MainWindowVM _parent;
    private bool _tagTypesLoaded;
    private TagType? _selectedType;
    private ObservableCollection<TagType> _tagTypes = new();
    private string? _title;
    private string? _synonyms;
    private bool _hasValue;
    private ICommand? _saveCommand;
    private ICommand? _resetCommand;

    public CreateTagVM(MainWindowVM _parentVM)
    {
        _parent = _parentVM;
    }


    public TagType? SelectedType
    {
        get => _selectedType;
        set => OnPropertyChanged(ref _selectedType, value, () => SelectedType);
    }

    public ObservableCollection<TagType> TagTypes => _tagTypes;

    public string? Title
    {
        get => _title;
        set
        {
            OnPropertyChanged(ref _title, value, () => Title);
        }
    }

    /// <summary>
    /// Separator :.:
    /// </summary>
    public string? Synonyms
    {
        get => _synonyms;
        set => OnPropertyChanged(ref _synonyms, value, () => Synonyms);
    }

    public List<string> SynonymsCollection 
        => _synonyms?.Split([":.:"], StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

    /// <summary>
    /// Separator :.:
    /// </summary>
    public bool HasValue
    {
        get => _hasValue;
        set => OnPropertyChanged(ref _hasValue, value, () => HasValue);
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

    private bool CanSave(object? obj) => SelectedType != null && !string.IsNullOrWhiteSpace(Title);

    private void Save(object? obj) => _parent.Tags.CreateTagAsync(this);

    public ICommand ResetCommand => _resetCommand ??= new RelayCommand(Reset);

    private void Reset(object? obj)
    {
        if (obj is bool && !(bool)obj)
            return;

        _tagTypesLoaded = false;
        ReloadTagTypesAsync();
        SelectedType = null;
        Synonyms = "";
        HasValue = false;
        Title = "";
    }


    private async void ReloadTagTypesAsync()
    {
        await ReloadTagTypesAsyncSemaphore.WaitAsync();

        try
        {
            if (_tagTypesLoaded)
            {
                return;
            }

            var tagTypes = await ReloadTagTypesTask();

            TagTypes.Clear();
            foreach (var tagType in tagTypes)
            {
                TagTypes.Add(tagType);
            }

            _tagTypesLoaded = true;
        }
        catch (Exception e)
        {
            TagTypesLoadFail(e);
        }
        finally
        {
            ReloadTagTypesAsyncSemaphore.Release();
        }
    }

    private void TagTypesLoadFail(Exception _)
    {
        _parent.View.ShowMessageDialog("Tag creating", "Unable to load TagTypes. Creating process terminated");
        _parent.View.CloseAllFlyouts();
    }

    private Task<List<TagType>> ReloadTagTypesTask()
    {
        // TODO load tag types
        //return Task.Run(() =>
        //{
        //    return ImoutoService.Use(imoutoService =>
        //    {
        //        return imoutoService.GetTagTypes();
        //    });
        //});
        return Task.FromResult(Array.Empty<TagType>().ToList());
    }
}
