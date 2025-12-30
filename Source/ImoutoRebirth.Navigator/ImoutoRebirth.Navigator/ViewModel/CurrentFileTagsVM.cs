using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.Utils;
using Newtonsoft.Json;
using Serilog;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class CurrentFileTagsVM : ObservableObject
{
    private Guid? _lastListEntryId;
    private int _rate;
    private bool _isFavorite;
    private readonly IFileTagService _fileTagService;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    public partial IReadOnlyCollection<DelayItem>? UgoiraFrameDelays { get; set; }

    [ObservableProperty]
    public partial bool IsRateSet { get; set; }

    [ObservableProperty]
    public partial bool ShowHotKeys { get; set; } = true;

    [ObservableProperty]
    private partial bool ForcedShowHotKeys { get; set; } = false;

    [ObservableProperty]
    public partial int? TagPixelWidth { get; set; }

    [ObservableProperty]
    public partial int? TagPixelHeight { get; set; }

    public CurrentFileTagsVM()
    {
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _messenger = ServiceLocator.GetMessenger();
    }

    public ObservableCollection<TagSourceVM> CurrentTagsSources { get; } = [];

    public int Rate
    {
        get => _rate;
        set
        {
            _rate = value;
            OnPropertyChanged();

            if (_lastListEntryId != null)
            {
                SetRate(value, _lastListEntryId.Value)
                    .OnException(ex =>
                    {
                        App.MainWindowVM?.SetStatusError("Error while setting rate", ex.Message);
                        Log.Error((Exception?)ex, "Error while setting rate");
                    });
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
                SetFavorite(value, _lastListEntryId.Value)
                    .OnException(ex =>
                    {
                        App.MainWindowVM?.SetStatusError("Error while setting favorite", ex.Message);
                        Log.Error(ex, "Error while setting favorite");
                    });
            }
        }
    }

    public async Task UpdateCurrentTags(Guid? fileId)
    {
        if (fileId == null)
        {
            IsRateSet = false;
            TagPixelWidth = null;
            TagPixelHeight = null;

            return;
        }

        var id = fileId.Value;

        var tags = await _fileTagService.GetFileTags(id);

        _lastListEntryId = id;

        var tagVmsCollection = tags
            .Select(x => new BindedTagVM(x, id, () => UpdateCurrentTags(_lastListEntryId)))
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
        GetPixelSize(tags);
        IsRateSet = true;

        SetShowHotKeys();
    }

    [RelayCommand]
    private void DraftAddTag(BindedTagVM tag)
    {
        _messenger.Send(new SelectTagInTagEditRequest(tag));
    }

    [RelayCommand]
    private void ToggleShowHotKeys()
    {
        ForcedShowHotKeys = !ForcedShowHotKeys;
        SetShowHotKeys();
    }

    [RelayCommand]
    private void SelectTagToSearch(BindedTagVM tag)
    {
        _messenger.Send(new SelectTagToSearchRequest(tag.Tag, tag.Value));
    }

    [RelayCommand]
    private void ExploreTag(BindedTagVM tag)
    {
        var tagName = WebUtility.UrlEncode(tag.Title.Replace(" ", "_"));
        Process.Start(new ProcessStartInfo($"https://danbooru.donmai.us/posts?tags={tagName}")
            { UseShellExecute = true });
    }

    private void SetShowHotKeys()
    {
        ShowHotKeys = ForcedShowHotKeys || CurrentTagsSources.None(x => Enumerable.Any<BindedTagVM>(x.Tags));
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

    private async Task SetRate(int value, Guid fileId)
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

    private void GetPixelSize(IReadOnlyCollection<FileTag> tags)
    {
        var widthTag = tags.FirstOrDefault(x => x.Tag is { Title: "width", HasValue: true });
        var heightTag = tags.FirstOrDefault(x => x.Tag is { Title: "height", HasValue: true });

        TagPixelWidth = widthTag?.Value != null && int.TryParse(widthTag.Value, out var width) ? width : null;
        TagPixelHeight = heightTag?.Value != null && int.TryParse(heightTag.Value, out var height) ? height : null;
    }

    private async Task SetFavorite(bool value, Guid fileId)
    {
        await _fileTagService.SetFavorite(fileId, value);
    }
}
