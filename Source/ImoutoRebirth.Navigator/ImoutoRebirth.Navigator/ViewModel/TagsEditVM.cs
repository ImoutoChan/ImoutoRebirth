using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Behaviors;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class TagsEditVM : VMBase, IDropable
{
    private readonly IFileTagService _fileTagService;
    private readonly ITagService _tagService;
    private readonly MainWindowVM _parentVm;

    private string? _searchText;
    private ICommand? _createTagCommand;
    private ICommand? _addTagsCommand;
    private ICommand? _removeTagsCommand;
    private ICommand? _saveCommand;
    private ICommand? _unbindCommand;
    private ICommand? _setTagInfoContextCommand;
    private CreateTagVM? _createTagVm;
    private bool _isSaving;
    private bool _isSuccess;
    private SearchTagVM? _tagInfoContext;

    public TagsEditVM(MainWindowVM parentVm)
    {
        parentVm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "SelectedEntries")
            {
                OnPropertyChanged(() => SelectedEntries);
            }
        };
        _parentVm = parentVm;

        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagService = ServiceLocator.GetService<ITagService>();
    }

    public ObservableCollection<SearchTagVM> FoundTags { get; } = new();

    public ObservableCollection<SearchTagVM> SelectedTags { get; } = new();

    public ObservableCollection<SearchTagVM> RecentlyTags { get; } = new();
    
    public ObservableCollection<SearchTagVM> UsersTopTags { get; } = new();

    public string? SearchText
    {
        get => _searchText;
        set
        {
            OnPropertyChanged(ref _searchText, value, () => SearchText);
            SearchTagsAsync();
        }
    }

    public SearchTagVM? TagInfoContext
    {
        get => _tagInfoContext;
        set => OnPropertyChanged(ref _tagInfoContext, value, () => TagInfoContext);
    }

    public IEnumerable<INavigatorListEntry> SelectedEntries => _parentVm.SelectedEntries;

    public IList SelectedItems => _parentVm.SelectedItems;

    public CreateTagVM? CreateTagVM
    {
        get => _createTagVm;
        private set => OnPropertyChanged(ref _createTagVm, value, () => CreateTagVM);
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

    public ICommand CreateTagCommand => _createTagCommand ??= new RelayCommand(CreateTag);

    private void CreateTag(object? _)
    {
        CreateTagVM = new CreateTagVM();
        CreateTagVM.RequestClosing += (_, _) => CreateTagVM = null;
    }

    public ICommand AddTagsCommand => _addTagsCommand ??= new RelayCommand(AddTags, CanAddTags);

    private bool CanAddTags(object? obj) => obj is IList<SearchTagVM> or SearchTagVM;

    private void AddTags(object? obj)
    {
        var tagVms = obj switch
        {
            IList<SearchTagVM> searchTagVms => searchTagVms.ToArray(),
            SearchTagVM searchTagVm => searchTagVm.AsArray(),
            _ => null
        };
        
        if (tagVms == null)
            return;

        var newTagVms = tagVms.Where(t => !SelectedTags.Any(x => x.Tag.Id == t.Tag.Id && x.Value == t.Value));

        foreach (var tagVm in newTagVms)
        {
            var value = tagVm.Tag.IsCounter ? "Counter:1" : null;
            tagVm.Value = value;
            SelectedTags.Add(tagVm);
        }
    }

    public ICommand RemoveTagsCommand => _removeTagsCommand ??= new RelayCommand(RemoveTags, CanRemoveTags);

    private bool CanRemoveTags(object? obj) => obj is IList<SearchTagVM> or SearchTagVM;

    private void RemoveTags(object? obj)
    {
        var tagVms = obj switch
        {
            IList<SearchTagVM> searchTagVms => searchTagVms.ToArray(),
            SearchTagVM searchTagVm => searchTagVm.AsArray(),
            _ => null
        };
        
        if (tagVms == null)
            return;

        foreach (var tagVm in tagVms)
        {
            var tagToRemove = SelectedTags.FirstOrDefault(x => x.Tag.Id == tagVm.Tag.Id && x.Value == tagVm.Value);
            
            if (tagToRemove != null) 
                SelectedTags.Remove(tagToRemove);
        }
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

    private bool CanSave(object? obj) => SelectedEntries.Any() && SelectedTags.Any();

    private async void Save(object? obj)
    {
        var images = SelectedEntries;
        var tags = SelectedTags;

        try
        {
            IsSaving = true;
            IsSuccess = false;

            var imageIds = images
                .Where(x => x.DbId.HasValue)
                .Select(x => x.DbId!.Value);

            var fileTags =
                from imageId in imageIds
                from tag in tags
                select new FileTag(
                    imageId,
                    tag.Tag,
                    tag.Tag.HasValue
                        ? tag.Value
                        : null,
                    FileTagSource.Manual);

            await _fileTagService.BindTags(fileTags.ToArray());

            IsSaving = false;

            UpdateRecentlyTags(SelectedTags);

            IsSuccess = true;
            await Task.Delay(500);
            IsSuccess = false;
        }
        catch
        {
            IsSaving = false;
        }
    }

    public ICommand UnbindCommand => _unbindCommand ??= new RelayCommand(Unbind, CanUnbind);

    private bool CanUnbind(object? obj) => SelectedEntries.Any() && SelectedTags.Any();

    private async void Unbind(object? obj)
    {
        var images = SelectedEntries;
        var tags = SelectedTags;

        try
        {
            IsSaving = true;
            IsSuccess = false;

            var imageIds = images
                .Where(x => x.DbId.HasValue)
                .Select(x => x.DbId!.Value);

            var fileTagsToUnbind =
                from imageId in imageIds
                from tag in tags
                select new UnbindTagRequest(
                    imageId,
                    tag.Tag.Id,
                    default,
                    FileTagSource.Manual);

            await _fileTagService.UnbindTags(fileTagsToUnbind.ToArray());

            IsSaving = false;
            IsSuccess = true;
            await Task.Delay(500);
            IsSuccess = false;
        }
        catch
        {
            IsSaving = false;
        }
    }

    private void UpdateRecentlyTags(IEnumerable<SearchTagVM> selectedTags)
    {
        foreach (var selectedTag in selectedTags)
        {
            var element = RecentlyTags
                .FirstOrDefault(x => x.Tag.Id == selectedTag.Tag.Id && x.Value == selectedTag.Value);

            if (element == null)
            {
                RecentlyTags.Insert(0, selectedTag);
            }
            else
            {
                RecentlyTags.Move(RecentlyTags.IndexOf(element), 0);
            }
        }
    }

    public async Task UpdateUsersTopTags()
    {
        var popular = await _tagService.GetPopularUserTags(20);
        var popularCharacters = await _tagService.GetPopularUserCharacterTags(20);
        
        UsersTopTags.Clear();

        foreach (var tag in popular) 
            UsersTopTags.Add(new SearchTagVM(new SearchTag(tag, null)));
        
        foreach (var tag in popularCharacters) 
            UsersTopTags.Add(new SearchTagVM(new SearchTag(tag, null)));
    }

    public ICommand SetTagInfoContextCommand => _setTagInfoContextCommand ??= new RelayCommand(SetTagInfoContext);

    private void SetTagInfoContext(object? obj) => TagInfoContext = obj as SearchTagVM;

    public void DraftAddTag(BindedTagVM tag)
    {
        var value = tag.Tag.IsCounter ? "Counter:1" : null;
        
        var searchTagVm = new SearchTagVM(new SearchTag(tag.Tag, value));
        SelectedTags.Add(searchTagVm);
    }
    
    private async void SearchTagsAsync()
    {
        var searchPattern = SearchText;

        if (string.IsNullOrWhiteSpace(searchPattern))
            return;

        try
        {
            var tags = await _tagService.SearchTags(searchPattern, 10);

            if (SearchText == searchPattern) 
                ReloadFoundTags(tags);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Tags load error: " + ex.Message);
        }
    }

    private void ReloadFoundTags(IReadOnlyCollection<Tag> tags)
    {
        FoundTags.Clear();
        foreach (var tag in tags)
        {
            FoundTags.Add(new SearchTagVM(new SearchTag(tag, null)));
        }
    }

    public Type DataType => typeof (List<SearchTagVM>);

    public void Drop(object data, int index = -1)
    {
        var boundTags = (data as List<SearchTagVM>)?.ToList();
        
        if (boundTags == null || !boundTags.Any())
            return;

        foreach (var boundTag in boundTags)
        {
            if (SelectedTags.Any(x => x.Tag.Id == boundTag.Tag.Id && x.Value == boundTag.Value))
                continue;

            SelectedTags.Add(boundTag);
        }
    }
}
