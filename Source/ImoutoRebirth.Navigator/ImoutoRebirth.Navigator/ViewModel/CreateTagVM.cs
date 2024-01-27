using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using TagType = ImoutoRebirth.Navigator.Services.Tags.Model.TagType;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class CreateTagVM : VMBase
{
    private static readonly SemaphoreSlim ReloadTagTypesAsyncSemaphore = new(1, 1);
    
    private bool _tagTypesLoaded;
    private bool _isSaving;
    private bool _isSuccess;
    private string? _title;
    private TagType? _selectedType;
    private string? _synonyms;
    private bool _hasValue;
    private bool _isCounter;
    private ICommand? _saveCommand;
    private ICommand? _cancelCommand;
    private readonly ITagService _tagService;

    public CreateTagVM()
    {
        _tagService = ServiceLocator.GetService<ITagService>();
        ReloadTagTypesAsync();
    }

    public TagType? SelectedType
    {
        get => _selectedType;
        set => OnPropertyChanged(ref _selectedType, value, () => SelectedType);
    }

    public ObservableCollection<TagType> TagTypes { get; } = new ObservableCollection<TagType>();

    public string? Title
    {
        get => _title;
        set => OnPropertyChanged(ref _title, value, () => Title);
    }

    /// <summary>
    ///     Separator :.:
    /// </summary>
    public string? Synonyms
    {
        get => _synonyms;
        set => OnPropertyChanged(ref _synonyms, value, () => Synonyms);
    }

    public List<string> SynonymsCollection 
        => _synonyms
               ?.Split(new[] { ":.:" }, StringSplitOptions.RemoveEmptyEntries)
               .ToList() 
           ?? new List<string>();

    public bool HasValue
    {
        get => _hasValue;
        set => OnPropertyChanged(ref _hasValue, value, () => HasValue);
    }

    public bool IsCounter
    {
        get => _isCounter;
        set
        {
            OnPropertyChanged(ref _isCounter, value, () => IsCounter);

            if (value)
                HasValue = true;
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => OnPropertyChanged(ref _isSaving, value, () => IsSaving);
    }

    public bool IsSuccess
    {
        get => _isSuccess;
        set => OnPropertyChanged(ref _isSuccess, value, () => IsSuccess);
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

    private bool CanSave(object? _) => SelectedType != null && !string.IsNullOrWhiteSpace(Title);

    private async void Save(object? _)
    {
        try
        {
            IsSaving = true;
            await CreateTagTask(this);
            IsSaving = false;
            IsSuccess = true;
            await Task.Delay(500);
            OnRequestClosing();

            Debug.WriteLine("Tag creating", "Tag successfully created");
        }
        catch (Exception ex)
        {
            IsSaving = false;

            Debug.WriteLine("Tag creating", "Error while creating: " + ex.Message);
        }
    }


    public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(Cancel);

    private void Cancel(object? _) => OnRequestClosing();

    private async void ReloadTagTypesAsync()
    {
        await ReloadTagTypesAsyncSemaphore.WaitAsync();

        try
        {
            if (_tagTypesLoaded)
                return;

            var tagTypes = await ReloadTagTypesTask();

            TagTypes.Clear();
            foreach (var tagType in tagTypes)
            {
                TagTypes.Add(tagType);
            }

            _tagTypesLoaded = true;
        }
        catch
        {
            Debug.WriteLine("Tag creating", "Unable to load TagTypes. Creating process terminated");
        }
        finally
        {
            ReloadTagTypesAsyncSemaphore.Release();
        }
    }

    private async Task<IReadOnlyCollection<TagType>> ReloadTagTypesTask() => await _tagService.GеtTypes();

    private async Task CreateTagTask(CreateTagVM createTagVm)
    {
        await _tagService.CreateTag(
            createTagVm.SelectedType?.Id ?? throw new InvalidOperationException("SelectedType is null"),
            createTagVm.Title ?? throw new InvalidOperationException("Title is null"),
            createTagVm.HasValue,
            createTagVm.SynonymsCollection,
            createTagVm.IsCounter);
    }

    public event EventHandler? RequestClosing;

    private void OnRequestClosing()
    {
        var handler = RequestClosing;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
