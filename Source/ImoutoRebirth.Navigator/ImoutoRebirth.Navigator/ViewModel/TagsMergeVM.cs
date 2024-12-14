using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using Serilog;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagsMergeVM : ObservableObject
{
    private readonly ITagService _tagService;

    private string? _searchText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeTagsCommand))]
    public partial SearchTagVM? TagToClean { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeTagsCommand))]
    public partial SearchTagVM? TagToEnrich { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCleanedTagCommand))]
    public partial SearchTagVM? CleanedTag { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SearchTagVM> FoundTags { get; set; } = new();

    [ObservableProperty]
    public partial bool IsInProgress { get; set; }

    [ObservableProperty]
    public partial bool IsSuccess { get; set; }

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
}
