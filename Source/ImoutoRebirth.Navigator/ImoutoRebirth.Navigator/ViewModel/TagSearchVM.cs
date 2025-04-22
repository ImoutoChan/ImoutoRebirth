using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.Utils;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice;
using Newtonsoft.Json;
using Serilog;
using SearchType = ImoutoRebirth.Navigator.Services.Tags.Model.SearchType;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagSearchVM : ObservableObject
{
    private string? _searchString;
    private List<string>? _comparators;
    private KeyValuePair<string, Guid?> _selectedCollection;
    private int _rate;
    private Guid? _lastListEntryId;
    private bool _isFavorite;
    private readonly IFileTagService _fileTagService;
    private readonly ITagService _tagService;
    private Tag? _favoriteTag;
    private Tag? _rateTag;

    [ObservableProperty]
    public partial bool ValueEnterMode { get; set; }

    [ObservableProperty]
    public partial string? SelectedComparator { get; set; }

    [ObservableProperty]
    public partial IReadOnlyCollection<DelayItem>? UgoiraFrameDelays { get; set; }

    [ObservableProperty]
    public partial bool IsRateSet { get; set; }

    [ObservableProperty]
    public partial string? EnteredValue { get; set; }

    [ObservableProperty]
    public partial Tag? SelectedHintBoxTag { get; set; }

    [ObservableProperty]
    private bool _showHotKeys = true;

    [ObservableProperty]
    private bool _forcedShowHotKeys = false;

    public TagSearchVM()
    {
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _tagService = ServiceLocator.GetService<ITagService>();

        Collections.Add(new KeyValuePair<string, Guid?>("All", null));

        SelectedCollection = Collections.First();

        ResetValueEnter();
    }

    public ObservableCollection<TagSourceVM> CurrentTagsSources { get; } = new();

    public ObservableCollection<KeyValuePair<string, Guid?>> Collections { get; } = new();

    public KeyValuePair<string, Guid?> SelectedCollection
    {
        get => _selectedCollection;
        set
        {
            _selectedCollection = value;
            OnPropertyChanged();
            OnSelectedCollectionChanged();
        }
    }

    public ObservableCollection<Tag> HintBoxTags { get; } = new();

    public ObservableCollection<SearchTagVM> SelectedBindedTags { get; } = new();

    public string? SearchString
    {
        get => _searchString;
        set
        {
            if (ValueEnterMode && value != _searchString)
            {
                ResetValueEnter();
            }

            if (string.IsNullOrWhiteSpace(value) || ValueEnterMode)
            {
                HintBoxTags.Clear();
            }
            else
            {
                SearchTagsAsync(value);
            }

            _searchString = value;
            OnPropertyChanged();
        }
    }

    public List<string> Comparators
    {
        get
        {
            return _comparators ??= ComparatorExtensions.GetValues<Comparator>()
                .Select(x => x.ToFriendlyString())
                .ToList();
        }
    }

    private Tag? EditedTag { get; set; }

    public int Rate
    {
        get => _rate;
        set
        {
            _rate = value;
            OnPropertyChanged();

            if (_lastListEntryId != null)
            {
                SetRate(value, _lastListEntryId.Value);
            }
        }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            OnPropertyChanged();

            if (_lastListEntryId != null)
            {
                SetFavorite(value, _lastListEntryId.Value);
            }
        }
    }

    public void AddCollections(ObservableCollection<CollectionVM> collections)
    {
        foreach (var collectionVm in collections)
        {
            Collections.Add(new KeyValuePair<string, Guid?>(collectionVm.Name, collectionVm.Id));
        }
    }

    public async void UpdateCurrentTags(Guid? fileId)
    {
        if (fileId == null)
        {
            IsRateSet = false;

            return;
        }

        var id = fileId.Value;

        var tags = await _fileTagService.GetFileTags(id);

        _lastListEntryId = id;

        var tagVmsCollection = tags
            .Select(x =>  new BindedTagVM(x, id, () => UpdateCurrentTags(_lastListEntryId)))
            .ToList();

        CurrentTagsSources.Clear();

        var userTags = tagVmsCollection
            .Where(x => x.Model.Source == FileTagSource.Manual)
            .ToList();

        if (userTags.Any())
        {
            CurrentTagsSources.Add(new TagSourceVM(
                "User",
                new ObservableCollection<BindedTagVM>(userTags
                    .OrderBy(x => x.TypePriority)
                    .ThenBy(x => x.Tag.Title))));
        }

        var parsedSources = tagVmsCollection.Select(x => x.Model.Source)
            .Where(x => x != FileTagSource.Manual)
            .OrderBy(x => x)
            .Distinct();

        foreach (var parsedSource in parsedSources)
        {
            CurrentTagsSources.Add(new TagSourceVM(
                parsedSource.ToString(),
                new ObservableCollection<BindedTagVM>(tagVmsCollection
                    .Where(x => Settings.Default.ShowSystemTags || x.Tag.Type.Title != "LocalMeta")
                    .Where(x => x.Model.Source == parsedSource)
                    .OrderBy(x => x.TypePriority)
                    .ThenBy(x => x.Tag.Title))));
        }

        GetFavorite(tags);
        GetRate(tags);
        GetUgoiraFrameData(tags);
        IsRateSet = true;

        SetShowHotKeys();
    }

    private async void SearchTagsAsync(string searchString)
    {
        HintBoxTags.Clear();

        try
        {
            if (int.TryParse(searchString, out var number))
            {
                if (!ValueEnterMode)
                {
                    HintBoxTags.SortList(await SearchTagsAsyncTask("BooruPostId"));
                }
                
                return;
            }
            
            var tags = await SearchTagsAsyncTask(searchString);

            if (!ValueEnterMode)
            {
                HintBoxTags.SortList(tags);
            }
        }
        catch (Exception ex)
        {
            App.MainWindowVM?.SetStatusError("Error while searching tags", ex.Message);
            Log.Error(ex, "Error while searching tags");
        }
    }

    private Task<IReadOnlyCollection<Tag>> SearchTagsAsyncTask(string searchString) 
        => _tagService.SearchTags(searchString, 50);

    [RelayCommand]
    private void SelectTag(object? param)
    {
        var tag = param as Tag;

        if (tag == null)
        {
            return;
        }

        if (tag.Title == "BooruPostId" && int.TryParse(SearchString, out var number))
        {
            var value = $"={number}";

            SelectedBindedTags.Add(
                new SearchTagVM(
                    new SearchTag(
                        tag,
                        value)));
            
            SearchString = string.Empty;
            OnSelectedTagsUpdated();
            
            return;
        }
            
        
        if (tag.HasValue
            && !ValueEnterMode)
        {
            SearchString = tag.Title;
            ValueEnterMode = true;
            EditedTag = tag;
            return;
        }

        if (SelectedBindedTags.All(x => x.Tag.Id != tag.Id || x.Value != EnteredValue))
        {
            var value = (tag.HasValue && !string.IsNullOrWhiteSpace(EnteredValue))
                ? SelectedComparator + EnteredValue
                : null;

            SelectedBindedTags.Add(
                new SearchTagVM(
                    new SearchTag(
                        tag,
                        value)));
        }

        SearchString = string.Empty;
        OnSelectedTagsUpdated();
    }

    [RelayCommand]
    private void ExploreTag(object? param)
    {
        if (param is not BindedTagVM tag)
            return;

        var tagName = WebUtility.UrlEncode(tag.Title.Replace(" ", "_"));
        Process.Start(new ProcessStartInfo($"https://danbooru.donmai.us/posts?tags={tagName}")
            { UseShellExecute = true });
    }

    [RelayCommand]
    private void DraftAddTag(BindedTagVM? tag)
    {
        if (tag != null)
            OnDraftAddRequested(tag);
    }

    [RelayCommand]
    private async Task SelectStaticTag(object? param)
    {
        var staticMode = param as string;

        if (staticMode == null)
        {
            return;
        }

        var favTag = await LoadFavoriteTag();
        var rateTag = await LoadRateTag();

        switch (staticMode)
        {
            case "Favorite":
                SelectedBindedTags.Add(
                    new SearchTagVM(
                        new SearchTag(
                            favTag,
                            null)));
                break;
            case "Rated5":
                SelectedBindedTags.Add(
                    new SearchTagVM(
                        new SearchTag(
                            rateTag,
                            "=5")));
                break;
            case "Rated4":
                SelectedBindedTags.Add(
                    new SearchTagVM(
                        new SearchTag(
                            rateTag,
                            "=4")));
                break;
            case "Unrated":
                SelectedBindedTags.Add(
                    new SearchTagVM(
                        new SearchTag(
                            rateTag,
                            null,
                            SearchType.Exclude))
                );
                break;
        }

        SearchString = string.Empty;
        OnSelectedTagsUpdated();
    }

    private async Task<Tag> LoadFavoriteTag()
    {
        if (_favoriteTag != null)
            return _favoriteTag;

        _favoriteTag = (await _tagService.SearchTags("Favorite", 1)).First();

        return _favoriteTag;
    }

    private async Task<Tag> LoadRateTag()
    {
        if (_rateTag != null)
            return _rateTag;

        _rateTag = (await _tagService.SearchTags("Rate", 1)).First();

        return _rateTag;
    }

    [RelayCommand]
    private void UnselectTag(object? param)
    {
        var tag = param as SearchTagVM;

        if (tag == null)
        {
            return;
        }

        var tagInList = SelectedBindedTags.FirstOrDefault(x => x.Tag.Id == tag.Tag.Id && x.Value == tag.Value);
        if (tagInList != null)
        {
            SelectedBindedTags.Remove(tagInList);
        }

        OnSelectedTagsUpdated();
    }

    private void ResetValueEnter()
    {
        EditedTag = null;
        ValueEnterMode = false;
        SelectedComparator = Comparators.First();
        EnteredValue = string.Empty;
    }

    [RelayCommand]
    private void EnterValueOk(object? obj)
    {
        SelectTag(EditedTag);
    }

    [RelayCommand]
    private void InvertSearchType(object? param)
    {
        var tag = param as SearchTagVM;

        if (tag == null)
        {
            return;
        }

        var tagInList = SelectedBindedTags.FirstOrDefault(x => x.Tag.Id == tag.Tag.Id && x.Value == tag.Value);
        if (tagInList != null)
        {
            tagInList.SearchType = tagInList.SearchType == SearchType.Include
                ? SearchType.Exclude
                : SearchType.Include;
        }

        OnSelectedTagsUpdated();
    }

    [RelayCommand]
    private void SelectBindedTag(object? param)
    {
        var tag = param as BindedTagVM;

        if (tag == null)
        {
            return;
        }

        if (SelectedBindedTags.All(x => x.Tag.Id != tag.Tag.Id || x.Value != tag.Value))
        {
            SelectedBindedTags.Add(new SearchTagVM(new SearchTag(tag.Tag, tag.Value, SearchType.Include)));
        }

        OnSelectedTagsUpdated();
    }

    [RelayCommand]
    private void ToggleShowHotKeys()
    {
        ForcedShowHotKeys = !ForcedShowHotKeys;
        SetShowHotKeys();
    }

    private void SetShowHotKeys()
    {
        ShowHotKeys = ForcedShowHotKeys || CurrentTagsSources.None(x => x.Tags.Any());
    }

    private void GetRate(IReadOnlyCollection<FileTag> tags)
    {
        var rateTag = tags.FirstOrDefault(x => x.Tag is { Title: "Rate", HasValue: true });
        
        if (rateTag?.Value != null && int.TryParse(rateTag.Value, out var rate))
        {
            _rate = rate;
        }
        else
        {
            _rate = 0;
        }

        OnPropertyChanged(nameof(Rate));
    }

    private async void SetRate(int value, Guid fileId)
    {
        await _fileTagService.SetRate(fileId, new Rate(value));
    }

    private void GetFavorite(IReadOnlyCollection<FileTag> tags)
    {
        var favTag = tags.FirstOrDefault(x => x.Tag.Title == "Favorite");
        _isFavorite = favTag != null;
        OnPropertyChanged(nameof(IsFavorite));
    }

    private void GetUgoiraFrameData(IReadOnlyCollection<FileTag> tags)
    {
        var frameDataTag = tags.FirstOrDefault(x => x.Tag.Title == "UgoiraFrameData");

        if (frameDataTag == null || string.IsNullOrEmpty(frameDataTag.Value))
            return;

        var frameData = JsonConvert.DeserializeObject<UgoiraFrameData>(frameDataTag.Value)!;

        UgoiraFrameDelays = frameData.Data.Select(x => new DelayItem(x.Delay, x.File)).ToList();
    }

    private async void SetFavorite(bool value, Guid fileId)
    {
        await _fileTagService.SetFavorite(fileId, value);
    }

    public event EventHandler? SelectedTagsUpdated;

    private void OnSelectedTagsUpdated()
    {
        var handler = SelectedTagsUpdated;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SelectedCollectionChanged;

    private void OnSelectedCollectionChanged()
    {
        var handler = SelectedCollectionChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<BindedTagVM>? DraftAddRequested;

    private void OnDraftAddRequested(BindedTagVM tag)
    {
        var handler = DraftAddRequested;
        handler?.Invoke(this, tag);
    }
}

internal enum Comparator
{
    Equal,
    NotEqual
    //MoreOrEqual,
    //More,
    //LessOrEqual,
    //Less
}

internal static class ComparatorExtensions
{
    public static string ToFriendlyString(this Comparator me)
    {
        switch (me)
        {
            default:
            case Comparator.Equal:
                return "=";
            case Comparator.NotEqual:
                return "!=";
            //case Comparator.MoreOrEqual:
            //    return ">=";
            //case Comparator.More:
            //    return ">";
            //case Comparator.LessOrEqual:
            //    return "<=";
            //case Comparator.Less:
            //    return "<";
        }
    }

    public static Comparator GetEnumFromFriendlyName(this string me)
    {
        switch (me)
        {
            default:
            case "=":
                return Comparator.Equal;
            case "!=":
                return Comparator.NotEqual;
            //case ">=":
            //    return Comparator.MoreOrEqual;
            //case ">":
            //    return Comparator.More;
            //case "<=":
            //    return Comparator.LessOrEqual;
            //case "<":
            //    return Comparator.Less;
        }
    }

    public static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof (T))
            .Cast<T>();
    }
}

internal class UgoiraFrameData
{
    [JsonProperty("data")]
    public required IReadOnlyCollection<FrameData> Data { get; set; }

    [JsonProperty("content_type")]
    public required string ContentType { get; set; }

    public class FrameData
    {
        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("file")]
        public required string File { get; set; }
    }
}
