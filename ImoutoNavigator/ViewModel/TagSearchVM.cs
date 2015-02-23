using ImoutoNavigator.Commands;
using ImoutoNavigator.WCF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Utils;
using WCFExchageLibrary.Data;

namespace ImoutoNavigator.ViewModel
{
    class TagSearchVM : VMBase
    {
        #region Fields

        private MainWindowVM _mainWindowVM;
        private string _searchString;
        //private ObservableCollection<KeyValuePair<TagM, int>>   _tagListCurrent     = new ObservableCollection<KeyValuePair<TagM, int>>();

        #endregion Fields

        #region Constructors

        public TagSearchVM(MainWindowVM mainWindowVM)
        {
            this._mainWindowVM = mainWindowVM;
        }

        #endregion Constructors

        #region Properties

        //public ObservableCollection<KeyValuePair<TagM, int>> TagListTop
        //{
        //    get
        //    {
        //        return new ObservableCollection<KeyValuePair<TagM, int>>(ImageList.Cast<ImageEntryVM>().Select(x => x.ImageModel).GetTopTags());
        //    }
        //}

        //public ObservableCollection<KeyValuePair<TagM, int>> TagListCurrent
        //{
        //    get
        //    {
        //        return _tagListCurrent;
        //    }
        //    set
        //    {
        //        _tagListCurrent = value;
        //    }
        //}

        public ObservableCollection<Tag> HintBoxTags { get; } = new ObservableCollection<Tag>();

        public ObservableCollection<Tag> SelectedTags { get; } = new ObservableCollection<Tag>();

        public List<BindedTag> SelectedBindedTags
        {
            get
            {
                return this.SelectedTags.Select(x => new BindedTag { Tag = x }).ToList();
            }
        }

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                //TODO Add find by first letters, add autocomplition
                if (String.IsNullOrWhiteSpace(value))
                {
                    HintBoxTags.Clear();
                }
                else
                {
                    SearchTagsAsync(value);
                }

                OnPropertyChanged(ref _searchString, value, () => this.SearchString);
            }
        }

        #endregion Properties

        #region Commands

        private ICommand _selectTagCommand;
        public ICommand SelectTagCommand
        {
            get
            {
                return _selectTagCommand ?? (_selectTagCommand = new RelayCommand(SelectTag));
            }
        }

        private ICommand _unselectTagCommand;
        public ICommand UnselectTagCommand
        {
            get
            {
                return _unselectTagCommand ?? (_unselectTagCommand = new RelayCommand(UnselectTag));
            }
        }

        #endregion Commands

        #region Methods

        private async void SearchTagsAsync(string searchString)
        {
            HintBoxTags.Clear();

            var tags = await SearchTagsAsyncTask(searchString);

            Sorts.SortList(HintBoxTags, tags);
        }

        private Task<List<Tag>> SearchTagsAsyncTask(string searchString)
        {
            return Task.Run(() =>
            {
                return
                    ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.SearchTags(searchString);
                    });
            });
        }

        private void SelectTag(object param)
        {
            if (param == null)
            {
                return;
            }

            var tag = param as Tag;

            if (tag == null)
            {
                return;
            }

            if (SelectedTags.All(x => x.Id != tag.Id))
            {
                SelectedTags.Add(tag);
            }

            SearchString = String.Empty;
            OnSelectedTagsUpdated();
        }

        private void UnselectTag(object param)
        {
            if (param == null)
            {
                return;
            }

            var tag = param as Tag;

            if (tag == null)
            {
                return;
            }

            var tagInList = SelectedTags.FirstOrDefault(x => x.Id == tag.Id);
            if (tagInList != null)
            {
                SelectedTags.Remove(tagInList);
            }

            OnSelectedTagsUpdated();
        }

        #endregion Methods

        #region Events

        public event EventHandler SelectedTagsUpdated;

        private void OnSelectedTagsUpdated()
        {
            var handler = SelectedTagsUpdated;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        #endregion Events
    }
}
