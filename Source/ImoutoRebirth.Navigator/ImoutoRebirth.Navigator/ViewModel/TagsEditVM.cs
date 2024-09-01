using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Behaviors;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Serilog;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagsEditVM : ObservableObject, IDropable
{
    private readonly IFileTagService _fileTagService;
    private readonly ITagService _tagService;
    private readonly MainWindowVM _parentVm;

    private string? _searchText;

    [ObservableProperty]
    private CreateTagVM? _createTagVM;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSuccess;
    
    [ObservableProperty]
    private SearchTagVM? _tagInfoContext;

    public TagsEditVM(MainWindowVM parentVm)
    {
        parentVm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowVM.SelectedEntries))
            {
                OnPropertyChanged(nameof(SelectedEntries));
                SaveCommand.NotifyCanExecuteChanged();
                UnbindCommand.NotifyCanExecuteChanged();
            }

            if (args.PropertyName == nameof(MainWindowVM.SelectedItems))
            {
                OnPropertyChanged(nameof(SelectedItems));
                SaveCommand.NotifyCanExecuteChanged();
                UnbindCommand.NotifyCanExecuteChanged();
            }
        };

        _parentVm = parentVm;

        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagService = ServiceLocator.GetService<ITagService>();
        
        SelectedTags.CollectionChanged += (_, _) =>
        {
            SaveCommand.NotifyCanExecuteChanged();
            UnbindCommand.NotifyCanExecuteChanged();
        };
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
            _searchText = value;
            OnPropertyChanged();
            SearchTagsAsync();
        }
    }

    public IEnumerable<INavigatorListEntry> SelectedEntries => _parentVm.SelectedEntries;

    public IList SelectedItems => _parentVm.SelectedItems;

    [RelayCommand]
    private void CreateTag(object? _)
    {
        CreateTagVM = new CreateTagVM();
        CreateTagVM.RequestClosing += (_, _) => CreateTagVM = null;
    }

    private static bool CanAddTags(object? obj) => obj is IList<SearchTagVM> or SearchTagVM;

    [RelayCommand(CanExecute = nameof(CanAddTags))]
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

    private bool CanRemoveTags(object? obj) => obj is IList<SearchTagVM> or SearchTagVM;

    [RelayCommand(CanExecute = nameof(CanRemoveTags))]
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

    private bool CanSave(object? obj) => SelectedEntries.Any() && SelectedTags.Any();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(object? obj)
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

    private bool CanUnbind(object? obj) => SelectedEntries.Any() && SelectedTags.Any();

    [RelayCommand(CanExecute = nameof(CanUnbind))]
    private async Task Unbind(object? obj)
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

    [RelayCommand]
    private void SetTagInfoContext(object? obj) => TagInfoContext = obj as SearchTagVM;
    
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

    public void DraftAddTag(BindedTagVM tag)
    {
        var value = tag.Tag.IsCounter ? "Counter:1" : null;
        
        var searchTagVm = new SearchTagVM(new SearchTag(tag.Tag, value));

        if (SelectedTags.Any(x => x.Tag.Id == searchTagVm.Tag.Id && x.Value == searchTagVm.Value))
            return;

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
            Log.Error(ex, "Tags load error");
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
