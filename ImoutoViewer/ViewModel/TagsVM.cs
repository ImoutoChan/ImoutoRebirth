using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Imouto.Utils;
using Imouto.Viewer.Model;
using Imouto.Viewer.WCF;
using Imouto.WCFExchageLibrary.Data;

namespace Imouto.Viewer.ViewModel
{
    class TagsVM : VMBase
    {
        #region Fields

        private readonly MainWindowVM _parent;

        private string _currentPath = "";
        private bool _showTags;
        private bool _isLastSuccessConnected;
        private bool _showNotes;

        #endregion Fields

        #region Constructors

        public TagsVM(MainWindowVM mainVM)
        {
            _parent = mainVM;
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

        public int? CurrentId { get; private set; }

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
                            NotesReload(tagsAndNotes.Item2);
                            TagsReload(tagsAndNotes.Item1);
                        }
                    }
                }
            }
            catch (EndpointNotFoundException)
            {
                _isLastSuccessConnected = false;
                OnPropertyChanged(() => ShowTags);
            }
            catch (Exception ex)
            {
                _isLastSuccessConnected = false;
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

        private Task CreateTagTask(CreateTagVM createTagVM)
        {
            return Task.Run(() =>
            {
                ImoutoService.Use(imoutoService =>
                {
                    imoutoService.CreateTag(new Tag
                    {
                        HasValue = createTagVM.HasValue,
                        SynonymsCollection = createTagVM.SynonymsCollection,
                        Title = createTagVM.Title,
                        Type = createTagVM.SelectedType
                    });
                });
            });
        }

        private Task<Tuple<List<BindedTag>, List<NoteM>>> LoadTagsTask(string path)
        {
            return Task.Run(() =>
            {
                var id = ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.GetImageId(path);
                });

                if (!id.HasValue)
                {
                    var md5 = Util.GetMd5Checksum(new System.IO.FileInfo(path));
                    id = ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.GetImageId(md5: md5);
                    });
                }

                CurrentId = id;

                var tags = new List<BindedTag>();
                var notes = new List<NoteM>();
                if (CurrentId.HasValue)
                {
                    tags = ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.GetImageTags(CurrentId.Value);
                    });
                    notes = ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.GetImageNotes(CurrentId.Value)
                                            .Select(WcfMapper.MapNote)
                                            .ToList();
                    });
                }

                return new Tuple<List<BindedTag>, List<NoteM>>(tags, notes);
            });
        }


        private void TagsReload(List<BindedTag> tags)
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

        private void NotesReload(List<NoteM> notes)
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

                TagsCollection.Where(tag => parsedSources.All(x => TagsCollection.Any(tagg => tagg.Source == x && tagg.Title == tag.Title)))
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

                TagsCollection.Where(x => x.Source == source && (SourcesCollection.FirstOrDefault()
                                                                                  ?.TagsCollection
                                                                                  ?.All(y => y.Title != x.Title) ?? false))
                              .ForEach(x => SourcesCollection.Last()
                                                             .TagsCollection.Add(x));
            }
        }

        private Task UnbindTagTask(int imageId, int tagId)
        {
            return Task.Run(() =>
            {
                ImoutoService.Use(imoutoService =>
                {
                    imoutoService.UnbindTag(imageId, tagId);
                });
            });
        }

        private Task BindTagTask(AddTagVM addTagVM)
        {
            return Task.Run(() =>
            {
                ImoutoService.Use(imoutoService =>
                {
                    imoutoService.BindTag(CurrentId.Value, new BindedTag
                    {
                        Tag = addTagVM.SelectedTag,
                        DateAdded = DateTime.Now,
                        Source = Source.User,
                        Value = (addTagVM.SelectedTag.HasValue)
                                ? addTagVM.Value
                                : null
                    });
                });
            });
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
}
