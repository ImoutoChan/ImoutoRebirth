using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice;
using Newtonsoft.Json;
using Serilog;
using System.Collections.ObjectModel;
using SearchType = ImoutoRebirth.Navigator.Services.Tags.Model.SearchType;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagSearchVM : ObservableObject, IRecipient<SelectTagToSearchRequest>
{
    private const string SilentValueEnterFlag = "!#!#!forced!#!#!";

    private readonly ITagService _tagService;
    private Tag? _favoriteTag;
    private Tag? _rateTag;

    [ObservableProperty]
    public partial bool ValueEnterMode { get; set; }

    [ObservableProperty]
    public partial string? SelectedComparator { get; set; }

    [ObservableProperty]
    public partial Tag? SelectedHintBoxTag { get; set; }

    [ObservableProperty]
    public partial string? SelectedHintBoxValue { get; set; }

    public TagSearchVM()
    {
        _tagService = ServiceLocator.GetService<ITagService>();
        ServiceLocator.GetMessenger().RegisterAll(this);

        Collections.Add(new("All", null));

        SelectedCollection = Collections.First();

        ResetValueEnter();
    }

    public ObservableCollection<KeyValuePair<string, Guid?>> Collections { get; } = [];

    public KeyValuePair<string, Guid?> SelectedCollection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnSelectedCollectionChanged();
        }
    }

    public ObservableCollection<Tag> HintBoxTags { get; } = [];

    public ObservableCollection<string> HintBoxValues { get; } = [];

    public ObservableCollection<SearchTagVM> SelectedBindedTags { get; } = [];

    public string? SearchString
    {
        get;
        set
        {
            if (ValueEnterMode && value != field)
            {
                ResetValueEnter();
            }

            if (string.IsNullOrWhiteSpace(value) || ValueEnterMode)
            {
                HintBoxTags.Clear();
            }
            else
            {
                SearchTagsAsync(value)
                    .OnException(ex =>
                    {
                        App.MainWindowVM?.SetStatusError("Error while searching tags", ex.Message);
                        Log.Error(ex, "Error while searching tags");
                    });
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public string? EnteredValue
    {
        get;
        set
        {

            if (string.IsNullOrWhiteSpace(value))
            {
                HintBoxValues.Clear();
            }
            else
            {
                if (value.StartsWith(SilentValueEnterFlag))
                {
                    value = value[SilentValueEnterFlag.Length..];
                }
                else
                {
                    SearchTagValuesAsync(value)
                        .OnException(ex =>
                        {
                            App.MainWindowVM?.SetStatusError("Error while searching tag values", ex.Message);
                            Log.Error(ex, "Error while searching tag values");
                        });
                }
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public List<string> Comparators 
        => field ??= ComparatorExtensions.GetValues<Comparator>()
            .Select(x => x.ToFriendlyString())
            .ToList();

    private Tag? EditedTag { get; set; }

    public void AddCollections(ObservableCollection<CollectionVM> collections)
    {
        Collections.Clear();
        Collections.Add(new("All", null));
        SelectedCollection = Collections.First();
        foreach (var collectionVm in collections)
        {
            Collections.Add(new(collectionVm.Name, collectionVm.Id));
        }
    }

    private async Task SearchTagsAsync(string searchString)
    {
        HintBoxTags.Clear();

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

    private Task<IReadOnlyCollection<Tag>> SearchTagsAsyncTask(string searchString) 
        => _tagService.SearchTags(searchString, 50);

    private async Task SearchTagValuesAsync(string? searchString)
    {
        if (EditedTag?.Id == null)
            return;

        HintBoxValues.Clear();

        var values = await _tagService.SearchTagValues(EditedTag.Id, searchString, 10);
        HintBoxValues.SortList(values);
    }

    [RelayCommand]
    private async Task SelectTag(Tag tag)
    {
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
            await SearchTagValuesAsync(null);
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
    private void SelectTagValue(string tagValue)
    {
        if (ValueEnterMode)
        {
            EnteredValue = SilentValueEnterFlag + tagValue;
            HintBoxValues.Clear();
        }
    }

    [RelayCommand]
    private async Task SelectStaticTag(string staticMode)
    {
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
    private void UnselectTag(SearchTagVM tag)
    {
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
    private async Task EnterValueOk()
    {
        if (EditedTag != null)
            await SelectTag(EditedTag);
    }

    [RelayCommand]
    private void InvertSearchType(SearchTagVM tag)
    {
        var tagInList = SelectedBindedTags.FirstOrDefault(x => x.Tag.Id == tag.Tag.Id && x.Value == tag.Value);
        if (tagInList != null)
        {
            tagInList.SearchType = tagInList.SearchType == SearchType.Include
                ? SearchType.Exclude
                : SearchType.Include;
        }

        OnSelectedTagsUpdated();
    }

    public void Receive(SelectTagToSearchRequest request)
    {
        var (tag, value) = request;
        value = value?.StartsWith('=') == false ? '=' + value : value;

        if (SelectedBindedTags.All(x => x.Tag.Id != tag.Id || x.Value != value))
        {
            SelectedBindedTags.Add(new(new(tag, value)));
            OnSelectedTagsUpdated();
        }
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

internal record SelectTagToSearchRequest(Tag Tag, string? Value);