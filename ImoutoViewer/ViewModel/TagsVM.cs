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

        private MainWindowVM _mainVM;

        private int? _currentId;
        private string _currentPath = "";
        private bool _showTags;
        private bool _isLastSuccessConnected = false;

        #endregion Fields

        #region Constructors
        
        public TagsVM(MainWindowVM mainVM)
        {
            _mainVM = mainVM;
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

        #endregion Properties

        #region Public methods

        public async void ReloadAsync()
        {
            try
            {
                var path = _mainVM.CurrentLocalImage.Path;

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
                if (needReload)
                {
                    tags = await LoadTagsAsync(path);

                    lock (_currentPath)
                    {
                        if (_currentPath == path)
                        {
                            TagsReload(tags);
                        }
                    }
                }

                _isLastSuccessConnected = true;
            }
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                _isLastSuccessConnected = false;
                OnPropertyChanged(() => ShowTags);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private Task<List<BindedTag>> LoadTagsAsync(string path)
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

        #endregion Public methods

        #region Private methods

        private void TagsReload(List<BindedTag> tags)
        {
            TagsCollection.Clear();

            foreach (var tag in tags.Where(x => x.Tag.Type.Title != "localmeta"))
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
