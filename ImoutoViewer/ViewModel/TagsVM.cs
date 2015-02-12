using ImoutoViewer.WCF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using WCFExchageLibrary.Data;

namespace ImoutoViewer.ViewModel
{
    class TagsVM : VMBase
    {
        #region Fields

        private MainWindowVM _parent;

        private int? _currentId;
        private string _currentPath = "";
        private bool _showTags;
        private bool _isLastSuccessConnected = false;

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

        public int? CurrentId
        {
            get
            {
                return _currentId;
            }
        }

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

                List<BindedTag> tags = new List<BindedTag>();

                if (needReload || fullReload)
                {
                    tags = await LoadTagsTask(path);
                    _isLastSuccessConnected = true;

                    lock (_currentPath)
                    {
                        if (_currentPath == path)
                        {
                            TagsReload(tags);
                        }
                    }
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException)
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

        private Task<List<BindedTag>> LoadTagsTask(string path)
        {
            return Task.Run(() =>
            {
                var id = ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.GetImageId(path: path);
                });
                _currentId = id;

                var tags = new List<BindedTag>();
                if (_currentId.HasValue)
                {
                    tags = ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.GetImageTags(_currentId.Value);
                    });
                }

                return tags;
            });
        }

        private void TagsReload(List<BindedTag> tags)
        {
            TagsCollection.Clear();

            foreach (var tag in tags.Where(x => x.Source != Source.System))
            {
                TagsCollection.Add(new BindedTagVM(tag, this));
            }

            UpdateSources();

            OnPropertyChanged(() => ShowTags);
            OnTagsLoaded();
        }

        private void UpdateSources()
        {
            SourcesCollection.Clear();

            foreach (var source in TagsCollection.Select(x => x.Source).Distinct())
            {
                SourcesCollection.Add(new SourceVM { Title = source });

                TagsCollection.Where(x => x.Source == source).ForEach(x => SourcesCollection.Last().TagsCollection.Add(x));
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
                    imoutoService.BindTag(
                        CurrentId.Value,
                        new BindedTag
                        {
                            Tag = addTagVM.SelectedTag,
                            DateAdded = DateTime.Now,
                            Source = Source.User,
                            Value = (addTagVM.SelectedTag.HasValue) ? addTagVM.Value : null
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
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        #endregion Events
    }
}
