using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagsMergeVM : ObservableObject
{
    private readonly ITagService _tagService;

    private string? _searchText;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeTagsCommand))]
    private SearchTagVM? _tagToClean;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeTagsCommand))]
    private SearchTagVM? _tagToEnrich;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCleanedTagCommand))]
    private SearchTagVM? _cleanedTag;

    [ObservableProperty]
    private ObservableCollection<SearchTagVM> _foundTags = new();

    [ObservableProperty]
    private bool _isInProgress;

    [ObservableProperty]
    private bool _isSuccess;

    public TagsMergeVM() => _tagService = ServiceLocator.GetService<ITagService>();

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

    [RelayCommand]
    private void SelectTagToClean(SearchTagVM? x)
    {
        TagToClean = x;
        CleanedTag = null;
    }

    [RelayCommand]
    private void SelectTagToEnrich(SearchTagVM? x)
    {
        TagToEnrich = x;
    }


    private bool CanMergeTags(object? obj) => TagToClean != null && TagToEnrich != null;

    [RelayCommand(CanExecute = nameof(CanMergeTags))]
    private async Task MergeTags(object? obj)
    {
        var tagToClean = TagToClean;
        var tagToEnrich = TagToEnrich;

        if (tagToClean == null || tagToEnrich == null)
            return;

        try
        {
            IsInProgress = true;

            await _tagService.MergeTags(tagToClean.Tag.Id, tagToEnrich.Tag.Id);
            
            IsSuccess = true;
            await Task.Delay(500);
            IsSuccess = false;

            CleanedTag = TagToClean;
        }
        finally
        {
            IsInProgress = false;
        }
    }
    
    private bool CanDeleteCleanedTag(SearchTagVM? param) => CleanedTag != null && param == CleanedTag;
    
    [RelayCommand(CanExecute = nameof(CanDeleteCleanedTag))]
    private async Task DeleteCleanedTag(SearchTagVM? param)
    {
        var cleanedTag = CleanedTag;

        if (cleanedTag == null || param != CleanedTag)
            return;

        try
        {
            IsInProgress = true;

            await _tagService.DeleteTag(cleanedTag.Tag.Id);
            
            IsSuccess = true;
            await Task.Delay(500);
            IsSuccess = false;

            CleanedTag = null;
        }
        finally
        {
            IsInProgress = false;
        }
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
}
