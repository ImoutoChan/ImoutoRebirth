using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
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
    private CancellationTokenSource? _searchTagsCancellation;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty] 
    private IReadOnlyCollection<Tag> _foundTags = [];

    [ObservableProperty] 
    private ObservableCollection<Tag> _selectedTags = new();

    [ObservableProperty] 
    private ObservableCollection<TagsPack> _tagsPacks = new();

    [ObservableProperty]
    private bool? _isSelectedTagsApplicationSuccess = null;

    public QuickTaggingVM(MainWindowVM mainWindowVM)
    {
        _mainWindowVM = mainWindowVM;
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagsService = ServiceLocator.GetService<ITagService>();

        PropertyChanged += ReactOnPropertyChanged;
    }

    private IEnumerable<INavigatorListEntry> SelectedEntries => _mainWindowVM.SelectedEntries;

    [RelayCommand]
    private void SelectTag(Tag? tag)
    {
        if (tag != null && SelectedTags.None(x => x.Id == tag.Id))
        {
            SelectedTags.Add(tag);
            SearchText = null;
        }
    }

    [RelayCommand]
    private async Task SearchTags(string? searchText)
    {
        var cts = new CancellationTokenSource();
        var oldCts = _searchTagsCancellation;

        _searchTagsCancellation = cts;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            FoundTags = [];
            return;
        }

        var tags = await _tagsService.SearchTags(searchText, 10);

        oldCts?.Cancel();
        if (cts.IsCancellationRequested)
            return;

        FoundTags = tags;
    }

    [RelayCommand]
    private void CreatePack()
    {
        if (SelectedTags.None())
            return;

        TagsPacks.Add(
            new TagsPack(
                SelectedTags.ToList(),
                Guid.NewGuid(),
                TagsPacks.LastOrDefault()?.Key));

        SelectedTags.Clear();
    }

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

        try
        {
            await _fileTagService.BindTags(fileTags.ToArray());
            IsSelectedTagsApplicationSuccess = true;
            await Task.Delay(1500);
            IsSelectedTagsApplicationSuccess = null;
        }
        catch (Exception e)
        {
            IsSelectedTagsApplicationSuccess = false;
            await Task.Delay(1500);
            IsSelectedTagsApplicationSuccess = null;
            
            Debug.WriteLine(e.Message);
        }
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
    private async Task ApplyPack(char tagsPackKey)
    {
        var pack = TagsPacks.FirstOrDefault(x => x.Key == tagsPackKey);

        if (pack == null)
            return;

        var images = SelectedEntries.ToList();
        var tags = pack.Tags.ToList();

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

        try
        {
            await _fileTagService.BindTags(fileTags.ToArray());
            pack.IsSuccess = true;
            await Task.Delay(1500);
            pack.IsSuccess = null;
        }
        catch (Exception e)
        {
            pack.IsSuccess = false;
            await Task.Delay(1500);
            pack.IsSuccess = null;
            
            Debug.WriteLine(e.Message);
        }
    }

    [RelayCommand]
    private async Task UndoPack(char tagsPackKey)
    {
        var pack = TagsPacks.FirstOrDefault(x => x.Key == tagsPackKey);

        if (pack == null)
            return;

        var images = SelectedEntries.ToList();
        var tags = pack.Tags.ToList();

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

        try
        {
            await _fileTagService.UnbindTags(fileTagsToUnbind.ToArray());
            pack.IsSuccess = true;
            await Task.Delay(1500);
            pack.IsSuccess = null;
        }
        catch (Exception e)
        {
            pack.IsSuccess = false;
            await Task.Delay(1500);
            pack.IsSuccess = null;
            
            Debug.WriteLine(e.Message);
        }
    }

    private async void ReactOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            await SearchTagsCommand.ExecuteAsync(SearchText);
        }
    }
}

internal partial class TagsPack : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyCollection<Tag> _tags;

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private char _key;

    [ObservableProperty]
    private bool? _isSuccess = null;

    public TagsPack(IReadOnlyCollection<Tag> tags, Guid id, char? lastKey)
    {
        _tags = tags;
        _id = id;
        _key = !lastKey.HasValue ? AvailableKeys[0] : AvailableKeys[Array.IndexOf(AvailableKeys, lastKey.Value) + 1];
    }

    private static readonly char[] AvailableKeys = ("12345" + "_WERT" + "ASDFG" + "ZXCVB").ToCharArray();
}

internal class DesignQuickTaggingVM : QuickTaggingVM
{
    public DesignQuickTaggingVM() : base(null!)
    {
        SearchText = "solo";
        SelectedTags = new ObservableCollection<Tag>([
            new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
        ]);
        TagsPacks = new ObservableCollection<TagsPack>([
            new TagsPack(
                new[]
                {
                    new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                    new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                    new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                },
                Guid.NewGuid(),
                'A'),
            new TagsPack(
                new[]
                {
                    new Tag(Guid.NewGuid(), "solo 4", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                    new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                    new Tag(Guid.NewGuid(), "solo 6", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
                },
                Guid.NewGuid(),
                'B'),
        ]);
        FoundTags = new[]
        {
            new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 4", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
            new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
        };
    }
}
