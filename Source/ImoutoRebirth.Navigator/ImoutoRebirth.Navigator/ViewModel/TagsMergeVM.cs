using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class TagsMergeVM : VMBase
{
    private readonly ITagService _tagService;

    private string? _searchText;
    private ICommand? _mergeTagsCommand;
    private ICommand? _deleteCleanedTagCommand;
    
    private ICommand? _selectTagToCleanCommand;
    private ICommand? _selectTagToEnrichCommand;
    
    private SearchTagVM? _tagToClean;
    private SearchTagVM? _tagToEnrich;
    private SearchTagVM? _cleanedTag;
    
    private ObservableCollection<SearchTagVM> _foundTags = new();
    
    private bool _isInProgress;
    private bool _isSuccess;

    public TagsMergeVM()
    {
        _tagService = ServiceLocator.GetService<ITagService>();
    }

    public ObservableCollection<SearchTagVM> FoundTags
    {
        get => _foundTags;
        set => OnPropertyChanged(ref _foundTags, value, () => FoundTags);
    }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            OnPropertyChanged(ref _searchText, value, () => SearchText);
            SearchTagsAsync();
        }
    }

    public SearchTagVM? TagToClean
    {
        get => _tagToClean;
        set => OnPropertyChanged(ref _tagToClean, value, () => TagToClean);
    }
    
    public SearchTagVM? CleanedTag
    {
        get => _cleanedTag;
        set => OnPropertyChanged(ref _cleanedTag, value, () => CleanedTag);
    }

    public SearchTagVM? TagToEnrich
    {
        get => _tagToEnrich;
        set => OnPropertyChanged(ref _tagToEnrich, value, () => TagToEnrich);
    }

    public bool IsInProgress
    {
        get => _isInProgress;
        set => OnPropertyChanged(ref _isInProgress, value, () => IsInProgress);
    }

    public bool IsSuccess
    {
        get => _isSuccess;
        set => OnPropertyChanged(ref _isSuccess, value, () => IsSuccess);
    }

    public ICommand MergeTagsCommand => _mergeTagsCommand ??= new RelayCommand(MergeTags, CanMergeTags);
    
    public ICommand DeleteCleanedTagCommand => _deleteCleanedTagCommand ??= new RelayCommand(DeleteCleanedTag, CanDeleteCleanedTag);
    
    public ICommand SelectTagToCleanCommand
        => _selectTagToCleanCommand ??= new RelayCommand<SearchTagVM>(x =>
        {
            TagToClean = x;
            CleanedTag = null;
        });
    
    public ICommand SelectTagToEnrichCommand
        => _selectTagToEnrichCommand ??= new RelayCommand<SearchTagVM>(x => TagToEnrich = x);
    

    private bool CanMergeTags(object? obj) => TagToClean != null && TagToEnrich != null;

    private async void MergeTags(object? obj)
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
    
    private bool CanDeleteCleanedTag(object? obj) => CleanedTag != null && obj == CleanedTag;
    
    private async void DeleteCleanedTag(object? obj)
    {
        var cleanedTag = CleanedTag;

        if (cleanedTag == null || obj != CleanedTag)
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
