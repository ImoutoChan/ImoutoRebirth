using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Imouto.Utils.Core;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using ImoutoViewer.Model;

namespace ImoutoViewer.ViewModel;

internal class TagsVM : VMBase
{
    #region Fields

    private readonly MainWindowVM _parent;

    private string _currentPath = "";
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
        try
        {
            var path = _parent.CurrentLocalImage.Path;

            var needReload = false;
            lock (_currentPath)
            {
                if (_currentPath != path)
                {
                    _currentPath = path;
                    needReload = true;
                }
            }

            if (needReload || fullReload)
            {
                var tagsAndNotes = await LoadTagsTask(path);
                _isLastSuccessConnected = true;

                lock (_currentPath)
                {
                    if (_currentPath == path)
                    {
                        NotesReload(tagsAndNotes.Notes);
                        TagsReload(tagsAndNotes.Tags);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _isLastSuccessConnected = false;
            OnPropertyChanged(() => ShowTags);
            Debug.WriteLine(ex.Message);
        }
    }

    public async void CreateTagAsync(CreateTagVM createTagVM)
    {
        try
        {
            await CreateTagTask(createTagVM);

            _parent.View.ShowMessageDialog("Tag creating", "Tag succsessfully created");
        }
        catch (Exception ex)
        {
            _parent.View.ShowMessageDialog("Tag creating", "Error while creating: " + ex.Message);
        }

        _parent.View.CloseAllFlyouts();
    }

    public async void UnbindTagAsync(BindedTagVM bindedTagVM)
    {
        if (CurrentId == null)
        {
            _parent.View.ShowMessageDialog("Removing tag from current image", "Can't remove tag: image is not in database");
            return;
        }

        try
        {
            await UnbindTagTask(CurrentId.Value, bindedTagVM.Id);

            _parent.View.ShowMessageDialog("Removing tag from current image", "Tag succsessfully removed");
        }
        catch (Exception ex)
        {
            _parent.View.ShowMessageDialog("Removing tag from current image", "Error while removing: " + ex.Message);
        }

        ReloadAsync(true);
    }

    public async void BindTagAsync(AddTagVM addTagVM)
    {
        if (_parent.Tags.CurrentId == null)
        {
            _parent.View.ShowMessageDialog("Adding tag to current image", "Can't add tag: image is not in database");
            return;
        }

        addTagVM.IsEnabled = false;

        try
        {
            await BindTagTask(addTagVM);

            _parent.View.ShowMessageDialog("Adding tag to current image", "Tag succsessfully added");
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
        return _tagService.CreateTag(
            createTagVm.SelectedType.Id,
            createTagVm.Title,
            createTagVm.HasValue,
            createTagVm.SynonymsCollection);
    }

    private async Task<(IReadOnlyCollection<FileTag> Tags, IReadOnlyCollection<NoteM> Notes)> LoadTagsTask(string path)
    {
        var md5Hash = await new FileInfo(path).GetMd5ChecksumAsync();
        var files = await _fileService.SearchFiles(md5Hash);

        if (!files.Any())
        {
            return
            (
                Array.Empty<FileTag>().ToList(),
                Array.Empty<NoteM>().ToList()
            );
        }

        var file = files.First();

        var tags = await _fileTagService.GetFileTags(file.Id);

        return
        (
            tags,
            Array.Empty<NoteM>().ToList()
        );
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
            SourcesCollection.Add(new SourceVM
            {
                Title = "Common"
            });


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

        foreach (var source in TagsCollection.Select(x => x.Source)
                     .Distinct())
        {
            SourcesCollection.Add(new SourceVM
            {
                Title = source
            });

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

    private Task UnbindTagTask(Guid imageId, Guid tagId)
    {
        // TODO remove tag

        //return Task.Run(() =>
        //{
        //    ImoutoService.Use(imoutoService =>
        //    {
        //        imoutoService.UnbindTag(imageId, tagId);
        //    });
        //});

        return Task.CompletedTask;
    }

    private Task BindTagTask(AddTagVM addTagVM)
    {
        // TODO add tag
        //return Task.Run(() =>
        //{
        //    ImoutoService.Use(imoutoService =>
        //    {
        //        imoutoService.BindTag(CurrentId.Value, new BindedTag
        //        {
        //            Tag = addTagVM.SelectedTag,
        //            DateAdded = DateTime.Now,
        //            Source = Source.User,
        //            Value = (addTagVM.SelectedTag.HasValue)
        //                    ? addTagVM.Value
        //                    : null
        //        });
        //    });
        //});

        return Task.CompletedTask;
    }

    #endregion Private methods

    #region Events

    public event EventHandler TagsLoaded;

    private void OnTagsLoaded()
    {
        var handler = TagsLoaded;
        handler?.Invoke(this, new EventArgs());
    }

    #endregion Events
}