using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Serilog;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class QuickTaggingVM : ObservableObject
{
    private readonly MainWindowVM _mainWindowVM;
    private readonly ITagService _tagsService;
    private readonly IFileTagService _fileTagService;

    private CancellationTokenSource? _searchTagsCancellation;

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyCollection<Tag> FoundTags { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<Tag> SelectedTags { get; set; } = new();

    [ObservableProperty]
    public partial bool? IsSelectedTagsApplicationSuccess { get; set; } = null;

    public AvailableTagPacksSetsVM AvailableTagPacksSets { get; } = new();

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

        AvailableTagPacksSets.Selected.AddNext(SelectedTags.ToList());
        SelectedTags.Clear();
    }

    [RelayCommand]
    private void ClearSelectedTags() => SelectedTags.Clear();

    [RelayCommand]
    private void ClearTagPacks() => AvailableTagPacksSets.Selected.Clear();

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
            
            Log.Error(e, "Error while applying tags");
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
    private void RemoveSelectedTag(Tag tag) => SelectedTags.Remove(tag);

    [RelayCommand]
    private async Task ApplyPack(char tagsPackKey)
    {
        var pack = AvailableTagPacksSets.Selected.GetByKeyOrDefault(tagsPackKey);

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
            
            Log.Error(e, "Error while applying tags pack");
        }
    }

    [RelayCommand]
    private async Task UndoPack(char tagsPackKey)
    {
        var pack = AvailableTagPacksSets.Selected.GetByKeyOrDefault(tagsPackKey);

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
            
            Log.Error(e, "Error while undoing tags pack");
        }
    }

    [RelayCommand]
    private void RemovePack(TagsPackVM pack) => AvailableTagPacksSets.Selected.Remove(pack);

    [RelayCommand]
    private void SavePacksSets() => AvailableTagPacksSets.SaveSets();

    [RelayCommand]
    private void DeleteSelectedPacksSet() => AvailableTagPacksSets.DeleteSelected();

    [RelayCommand]
    private void RenameSelectedTagsPacksSet(string newTitle) => AvailableTagPacksSets.Selected.Rename(newTitle);

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