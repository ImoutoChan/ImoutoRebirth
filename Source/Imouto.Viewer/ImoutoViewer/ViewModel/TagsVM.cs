using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using ImoutoRebirth.Common.WPF;
using ImoutoViewer.ImoutoRebirth.Services;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel;

internal class TagsVM : VMBase
{
    #region Fields

    private readonly MainWindowVM _parent;

    private string? _currentPath;
    private CancellationTokenSource? _metadataRequest;
    private bool _showTags;
    private bool _isLastSuccessConnected;
    private bool _showNotes;

    private readonly IFileTagService _fileTagService;
    private readonly ITagService _tagService;
    private readonly IFileService _fileService;

    #endregion Fields

    #region Constructors

    public TagsVM(MainWindowVM mainVM)
    {
        _parent = mainVM;
        _fileTagService = ServiceLocator.GetRequiredService<IFileTagService>();
        _fileService = ServiceLocator.GetRequiredService<IFileService>();
        _tagService = ServiceLocator.GetRequiredService<ITagService>();
    }

    #endregion Constructors

    #region Properties

    public ObservableCollection<SourceVM> SourcesCollection { get; } = new ObservableCollection<SourceVM>();

    public ObservableCollection<BindedTagVM> TagsCollection { get; } = new ObservableCollection<BindedTagVM>();

    public ObservableCollection<NoteM> NotesCollection { get; } = new ObservableCollection<NoteM>();

    public bool ShowTags
    {
        get
        {
            return _showTags && _isLastSuccessConnected;
        }
        set
        {
            OnPropertyChanged(ref _showTags, value, () => ShowTags);
        }
    }

    public bool ShowNotes
    {
        get
        {
            return _showNotes;
        }
        set
        {
            OnPropertyChanged(ref _showNotes, value, () => ShowNotes);
        }
    }

    public Guid? CurrentId { get; private set; }

    #endregion Properties

    #region Public methods

    public async void ReloadAsync(bool fullReload = false)
    {
        var path = _parent.CurrentLocalImage?.Path;
        var pathChanged = path != _currentPath;
        if (!pathChanged && !fullReload)
            return;

        _metadataRequest?.Cancel();
        _currentPath = path;
        CurrentId = null;
        if (pathChanged)
        {
            NotesCollection.Clear();
            TagsCollection.Clear();
            SourcesCollection.Clear();
        }

        if (path == null)
            return;

        using var request = new CancellationTokenSource();
        _metadataRequest = request;
        try
        {
            var result = await Task.Run(() => LoadTagsTask(path, request.Token), request.Token);
            if (request.IsCancellationRequested)
                return;

            CurrentId = result.FileId;
            _isLastSuccessConnected = true;
            NotesReload(result.Notes);
            TagsReload(result.Tags);
        }
        catch (Exception ex)
        {
            if (request.IsCancellationRequested)
                return;

            _isLastSuccessConnected = false;
            OnPropertyChanged(() => ShowTags);
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            if (_metadataRequest == request)
                _metadataRequest = null;
        }
    }

    public async void CreateTagAsync(CreateTagVM createTagVM)
    {
        try
        {
            await CreateTagTask(createTagVM);

            _parent.View.ShowMessageDialog("Tag creating", "Tag successfully created");
        }
        catch (Exception ex)
        {
            _parent.View.ShowMessageDialog("Tag creating", "Error while creating: " + ex.Message);
        }

        _parent.View.CloseAllFlyouts();
    }

    public async void UnbindTagAsync(BindedTagVM bindedTagVM)
    {
        if (CurrentId != bindedTagVM.Model.FileId || !bindedTagVM.IsEditable)
        {
            _parent.View.ShowMessageDialog("Removing tag from current image", "Can't remove tag: image is not in database");
            return;
        }

        try
        {
            var tag = bindedTagVM.Model;
            await _fileTagService.UnbindTags(new UnbindTagRequest(tag.FileId, tag.Tag.Id, tag.Value, tag.Source));

            _parent.View.ShowMessageDialog("Removing tag from current image", "Tag successfully removed");
        }
        catch (Exception ex)
        {
            _parent.View.ShowMessageDialog("Removing tag from current image", "Error while removing: " + ex.Message);
        }

        ReloadAsync(true);
    }

    public async void BindTagAsync(AddTagVM addTagVM)
    {
        if (CurrentId is not { } fileId)
        {
            _parent.View.ShowMessageDialog("Adding tag to current image", "Can't add tag: image is not in database");
            return;
        }

        if (addTagVM.SelectedTag is not { } tag)
            return;

        addTagVM.IsEnabled = false;

        try
        {
            await _fileTagService.BindTags([new FileTag(fileId, tag, tag.HasValue ? addTagVM.Value : null, FileTagSource.Manual)]);

            _parent.View.ShowMessageDialog("Adding tag to current image", "Tag successfully added");
            _parent.View.CloseAllFlyouts();
        }
        catch (Exception ex)
        {
            _parent.View.ShowMessageDialog("Adding tag to current image", "Error while adding: " + ex.Message);

            addTagVM.IsEnabled = true;
        }

        _parent.Tags.ReloadAsync(true);
    }

    #endregion Public methods

    #region Private methods

    private Task CreateTagTask(CreateTagVM createTagVm)
    {
        if (createTagVm.SelectedType == null || createTagVm.Title == null)
            throw new ArgumentException("Tag type or title is null");
        
        return _tagService.CreateTag(
            createTagVm.SelectedType.Id,
            createTagVm.Title,
            createTagVm.HasValue,
            createTagVm.SynonymsCollection);
    }

    private async Task<(Guid? FileId, IReadOnlyCollection<FileTag> Tags, IReadOnlyCollection<NoteM> Notes)> LoadTagsTask(
        string path,
        CancellationToken ct)
    {
        string md5Hash;
        using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
        {
            var hash = await MD5.HashDataAsync(stream, ct);
            md5Hash = Convert.ToHexStringLower(hash);
        }

        var files = await _fileService.SearchFiles(md5Hash, ct);
        if (files.Count == 0)
            return (null, [], []);

        var file = files.First();
        var (tags, notes) = await _fileTagService.GetFileMetadata(file.Id, ct);
        var notesModels = notes.Select(x => new NoteM(x.Label!, x.PositionFromLeft,
            x.PositionFromTop, x.Width, x.Height, x.Source)).ToList();

        var canEdit = files.Count == 1 || string.Equals(file.Path, path, StringComparison.OrdinalIgnoreCase);
        return (canEdit ? file.Id : null, tags, notesModels);
    }

    private void TagsReload(IReadOnlyCollection<FileTag> tags)
    {
        TagsCollection.Clear();

        foreach (var tag in tags.Where(x => x.Tag.Type.Title != "LocalMeta"))
        {
            TagsCollection.Add(new BindedTagVM(tag, this));
        }

        UpdateSources();

        OnPropertyChanged(() => ShowTags);
        OnTagsLoaded();
    }

    private void NotesReload(IReadOnlyCollection<NoteM> notes)
    {
        notes = notes.GroupBy(x => x.Source).MinBy(x => x.Key switch
        {
            FileTagSource.Danbooru => 0,
            FileTagSource.Yandere => 1,
            FileTagSource.Sankaku => 2,
            FileTagSource.Rule34 => 3,
            FileTagSource.Gelbooru => 4,
            FileTagSource.ExHentai => 5,
            FileTagSource.Schale => 6,
            FileTagSource.Manual => 7,
            FileTagSource.Lamia => 8,
            _ => 9
        })?.ToArray() ?? [];
        
        NotesCollection.Clear();

        foreach (var note in notes)
        {
            NotesCollection.Add(note);
        }
    }

    private void UpdateSources()
    {
        SourcesCollection.Clear();

        var parsedSources = TagsCollection.Select(x => x.Source)
            .Where(x => x != "User")
            .Distinct().ToList();
        if (parsedSources.Count() > 1)
        {
            SourcesCollection.Add(new SourceVM("Common"));

            var commonBindedTagVms = SourcesCollection.Last()
                .TagsCollection;

            TagsCollection.Where(
                    tag => parsedSources.All(
                        x => TagsCollection.Any(tagg => tagg.Source == x && tagg.Title == tag.Title)))
                .ToList()
                .ForEach(x => commonBindedTagVms.Add(x));

            for (var i = 0; i < commonBindedTagVms.Count;)
            {
                if (commonBindedTagVms.Any(y => y.Title == commonBindedTagVms[i].Title && commonBindedTagVms.IndexOf(y) != i))
                {
                    commonBindedTagVms.Remove(commonBindedTagVms[i]);
                }
                else
                {
                    i++;
                }
            }

            if (!commonBindedTagVms.Any())
            {
                SourcesCollection.Clear();
            }
        }

        foreach (var source in TagsCollection.Select(x => x.Source).Distinct())
        {
            SourcesCollection.Add(new SourceVM(source));

            TagsCollection.Where(
                    x => x.Source == source && (SourcesCollection.FirstOrDefault()
                        ?.TagsCollection
                        ?.All(y => y.Title != x.Title) ?? false))
                .ToList()
                .ForEach(
                    x => SourcesCollection.Last()
                        .TagsCollection.Add(x));
        }
    }

    #endregion Private methods

    #region Events

    public event EventHandler? TagsLoaded;

    private void OnTagsLoaded() => TagsLoaded?.Invoke(this, EventArgs.Empty);

    #endregion Events
}
