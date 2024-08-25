using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using TagType = ImoutoRebirth.Navigator.Services.Tags.Model.TagType;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class CreateTagVM : ObservableObject
{
    private static readonly SemaphoreSlim ReloadTagTypesAsyncSemaphore = new(1, 1);
    
    private bool _tagTypesLoaded;
    private bool _isCounter;
    private readonly ITagService _tagService;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private TagType? _selectedType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _title;

    /// <summary>
    ///     Separator :.:
    /// </summary>
    [ObservableProperty]
    private string? _synonyms;

    [ObservableProperty]
    private bool _hasValue;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSuccess;

    public CreateTagVM()
    {
        _tagService = ServiceLocator.GetService<ITagService>();
        ReloadTagTypesAsync();
    }

    public ObservableCollection<TagType> TagTypes { get; } = new();

    public List<string> SynonymsCollection
        => Synonyms?.Split([":.:"], StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

    public bool IsCounter
    {
        get => _isCounter;
        set
        {
            _isCounter = value;
            OnPropertyChanged();

            if (value)
                HasValue = true;
        }
    }

    private bool CanSave() => SelectedType != null && !string.IsNullOrWhiteSpace(Title);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
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

    [RelayCommand]
    private void Cancel() => OnRequestClosing();

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
