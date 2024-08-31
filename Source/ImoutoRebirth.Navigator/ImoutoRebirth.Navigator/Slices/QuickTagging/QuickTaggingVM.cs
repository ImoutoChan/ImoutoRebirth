using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
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
    private TagsPacksVM _tagsPacks = new();

    [ObservableProperty]
    private bool? _isSelectedTagsApplicationSuccess = null;

    public QuickTaggingVM(MainWindowVM mainWindowVM)
    {
        _mainWindowVM = mainWindowVM;
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagsService = ServiceLocator.GetService<ITagService>();

        PropertyChanged += ReactOnPropertyChanged;
        _mainWindowVM.PropertyChanged += ReactOnMainWindowVMPropertyChanged;
    }

    private IEnumerable<INavigatorListEntry> SelectedMediaEntries => _mainWindowVM.SelectedEntries;

    public int SelectedMediaEntriesCount => _mainWindowVM.SelectedItems.Count;

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

        TagsPacks.AddNext(SelectedTags.ToList());
        SelectedTags.Clear();
    }

    [RelayCommand]
    private void ClearSelectedTags() => SelectedTags.Clear();

    [RelayCommand]
    private void ClearTagPacks() => TagsPacks.Clear();

    [RelayCommand]
    private async Task ApplySelectedTags()
    {
        var images = SelectedMediaEntries.ToList();
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
        var images = SelectedMediaEntries.ToList();
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
        var pack = TagsPacks.GetByKeyOrDefault(tagsPackKey);

        if (pack == null)
            return;

        var images = SelectedMediaEntries.ToList();
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
        var pack = TagsPacks.GetByKeyOrDefault(tagsPackKey);

        if (pack == null)
            return;

        var images = SelectedMediaEntries.ToList();
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

    [RelayCommand]
    private void RemovePack(TagsPackVM pack) => TagsPacks.Remove(pack);

    [RelayCommand]
    private void SavePacks()
    {
        var serialized = TagsPacks.Save();
        Settings.Default.TagsPacks = serialized;
        Settings.Default.Save();
    }
    
    [RelayCommand]
    private void LoadPacks()
    {
        var serialized = Settings.Default.TagsPacks;
        TagsPacks.Load(serialized);
    }

    private async void ReactOnPropertyChanged(object? _, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            await SearchTagsCommand.ExecuteAsync(SearchText);
        }
    }

    private void ReactOnMainWindowVMPropertyChanged(object? _, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowVM.SelectedEntries))
        {
            OnPropertyChanged(nameof(SelectedMediaEntries));
            OnPropertyChanged(nameof(SelectedMediaEntriesCount));
        }
    }
}

internal partial class TagsPacksVM : ObservableObject
{
    public ObservableCollection<TagsPackVM> Packs { get; } = new();

    public void AddNext(IReadOnlyCollection<Tag> tags)
    {
        var lastKey = Packs.LastOrDefault()?.Key;
        var nextKey = GetNextKey(lastKey);

        if (nextKey == '_')
        {
            Packs.Add(new EmptyTagPackVM());
            nextKey = GetNextKey(nextKey);
        }

        if (nextKey == '-')
        {
            Packs.Add(new EmptyTagPackVM());
            nextKey = GetNextKey(nextKey);
        }

        Packs.Add(new TagsPackVM(tags, nextKey));
    }

    private static char GetNextKey(char? lastKey)
    {
        return !lastKey.HasValue
            ? AvailableKeys[0]
            : AvailableKeys[Array.IndexOf(AvailableKeys, lastKey.Value) + 1];
    }

    private static readonly char[] AvailableKeys = ("12345" + "_WERT" + "ASDFG" + "Z-CVB").ToCharArray();

    public TagsPackVM? GetByKeyOrDefault(char key) 
        => Packs.FirstOrDefault(x => string.Equals(x.Key.ToString(), key.ToString(), StringComparison.OrdinalIgnoreCase));

    public void Clear() => Packs.Clear();

    public string Save() => JsonSerializer.Serialize(Packs.Select(x => x.Tags).ToList());

    public void Load(string serialized)
    {
        var tags = JsonSerializer.Deserialize<IReadOnlyCollection<IReadOnlyCollection<Tag>>>(serialized);

        if (tags == null)
            return;

        Clear();

        foreach (var pack in tags.Where(x => x.Any()))
            AddNext(pack);
    }

    public void Remove(TagsPackVM pack)
    {
        if (Packs.Contains(pack))
            Packs.Remove(pack);
    }
}
internal partial class TagsPackVM : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyCollection<Tag> _tags;

    [ObservableProperty]
    private char _key;

    [ObservableProperty]
    private bool? _isSuccess = null;

    public TagsPackVM(IReadOnlyCollection<Tag> tags, char key)
    {
        _tags = tags;
        _key = key;
    }
}

internal class EmptyTagPackVM : TagsPackVM
{
    public EmptyTagPackVM() : base([], '_')
    {
    }
}

//internal class DesignQuickTaggingVM : QuickTaggingVM
//{
//    public DesignQuickTaggingVM() : base(null!)
//    {
//        SearchText = "solo";
//        SelectedTags = new ObservableCollection<Tag>([
//            new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//        ]);
//        TagsPacks = new ObservableCollection<TagsPackVM>([
//            new TagsPackVM(
//                new[]
//                {
//                    new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                    new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                    new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                },
//                Guid.NewGuid(),
//                'A'),
//            new TagsPackVM(
//                new[]
//                {
//                    new Tag(Guid.NewGuid(), "solo 4", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                    new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                    new Tag(Guid.NewGuid(), "solo 6", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//                },
//                Guid.NewGuid(),
//                'B'),
//        ]);
//        FoundTags = new[]
//        {
//            new Tag(Guid.NewGuid(), "solo 1", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 2", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 3", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 4", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//            new Tag(Guid.NewGuid(), "solo 5", new TagType(Guid.NewGuid(), "general", 0), [], false, false, 0),
//        };
//    }
//}
