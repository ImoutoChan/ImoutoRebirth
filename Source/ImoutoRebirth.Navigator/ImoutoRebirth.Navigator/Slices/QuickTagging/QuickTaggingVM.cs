using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class QuickTaggingVM : ObservableObject
{
    private readonly MainWindowVM _mainWindowVM;
    private readonly ITagService _tagsService;
    private readonly IFileTagService _fileTagService;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty] 
    private IReadOnlyCollection<Tag> _foundTags = [];

    [ObservableProperty] 
    private ObservableCollection<Tag> _selectedTags = new();

    [ObservableProperty] 
    private ObservableCollection<TagsPack> _tagsPacks = new();

    public QuickTaggingVM(MainWindowVM mainWindowVM)
    {
        _mainWindowVM = mainWindowVM;
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagsService = ServiceLocator.GetService<ITagService>();
    }

    public IEnumerable<INavigatorListEntry> SelectedEntries => _mainWindowVM.SelectedEntries;

    [RelayCommand]
    private async Task SearchTags(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FoundTags = [];
            return;
        }

        var tags = await _tagsService.SearchTags(searchText, 10);
        FoundTags = tags;
    }

    [RelayCommand]
    private void CreatePack() => TagsPacks.Add(new TagsPack(SelectedTags.ToList(), Guid.NewGuid()));

    [RelayCommand]
    private void ClearSelectedTags() => SelectedTags.Clear();

    [RelayCommand]
    private async Task ApplySelectedTags()
    {
        var images = SelectedEntries.ToList();
        var tags = SelectedTags.ToList();

        var imageIds = images
            .Where(x => x.DbId.HasValue)
            .Select(x => x.DbId!.Value);

        var fileTags =
            from imageId in imageIds
            from tag in tags
            select new FileTag(
                imageId,
                tag,
                null,
                FileTagSource.Manual);

        await _fileTagService.BindTags(fileTags.ToArray());
    }

    [RelayCommand]
    private async Task UndoSelectedTags()
    {
        var images = SelectedEntries.ToList();
        var tags = SelectedTags.ToList();

        var imageIds = images
            .Where(x => x.DbId.HasValue)
            .Select(x => x.DbId!.Value);

        var fileTagsToUnbind =
            from imageId in imageIds
            from tag in tags
            select new UnbindTagRequest(
                imageId,
                tag.Id,
                null,
                FileTagSource.Manual);

        await _fileTagService.UnbindTags(fileTagsToUnbind.ToArray());
    }

    [RelayCommand]
    private async Task ApplyPack(TagsPack tagsPack)
    {
        var images = SelectedEntries.ToList();
        var tags = tagsPack.Tags.ToList();

        var imageIds = images
            .Where(x => x.DbId.HasValue)
            .Select(x => x.DbId!.Value);

        var fileTags =
            from imageId in imageIds
            from tag in tags
            select new FileTag(
                imageId,
                tag,
                null,
                FileTagSource.Manual);

        await _fileTagService.BindTags(fileTags.ToArray());
    }

    [RelayCommand]
    private async Task UndoPack(TagsPack tagsPack)
    {
        var images = SelectedEntries.ToList();
        var tags = tagsPack.Tags.ToList();

        var imageIds = images
            .Where(x => x.DbId.HasValue)
            .Select(x => x.DbId!.Value);

        var fileTagsToUnbind =
            from imageId in imageIds
            from tag in tags
            select new UnbindTagRequest(
                imageId,
                tag.Id,
                null,
                FileTagSource.Manual);

        await _fileTagService.UnbindTags(fileTagsToUnbind.ToArray());
    }
}

internal record TagsPack(IReadOnlyCollection<Tag> Tags, Guid Id);
