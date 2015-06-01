using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoNavigator.Commands;
using ImoutoNavigator.WCF;
using Utils;
using WCFExchageLibrary.Data;

namespace ImoutoNavigator.ViewModel
{
    internal class TagSearchVM : VMBase
    {
        #region Fields

        private MainWindowVM _mainWindowVM;
        private string _searchString;
        //private ObservableCollection<KeyValuePair<TagM, int>>   _tagListCurrent     = new ObservableCollection<KeyValuePair<TagM, int>>();
        private Tag _selectedHintBoxTag;
        private bool _isValueEnterMode;
        private string _enteredValue;
        private List<string> _comparators;
        private string _selectedComparator;

        #endregion Fields

        #region Constructors

        public TagSearchVM(MainWindowVM mainWindowVM)
        {
            _mainWindowVM = mainWindowVM;
            ResetValueEnter();
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

        public Tag SelectedHintBoxTag
        {
            get
            {
                return _selectedHintBoxTag;
            }
            set
            {
                OnPropertyChanged(ref _selectedHintBoxTag, value, () => SelectedHintBoxTag);
            }
        }

        public ObservableCollection<BindedTagVM> SelectedBindedTags { get; } = new ObservableCollection<BindedTagVM>();

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                if (ValueEnterMode && value != _searchString)
                {
                    ResetValueEnter();
                }

                if (String.IsNullOrWhiteSpace(value) || ValueEnterMode)
                {
                    HintBoxTags.Clear();
                }
                else
                {
                    SearchTagsAsync(value);
                }

                OnPropertyChanged(ref _searchString, value, () => SearchString);
            }
        }

        public bool ValueEnterMode
        {
            get
            {
                return _isValueEnterMode;
            }
            set
            {
                OnPropertyChanged(ref _isValueEnterMode, value, () => ValueEnterMode);
            }
        }

        public List<string> Comparators
        {
            get
            {
                return _comparators
                       ?? (_comparators =
                           ComparatorExtensions.GetValues<Comparator>().Select(x => x.ToFriendlyString()).ToList());
            }
        }

        public string SelectedComparator
        {
            get
            {
                return _selectedComparator;
            }
            set
            {
                OnPropertyChanged(ref _selectedComparator, value, () => SelectedComparator);
            }
        }

        public string EnteredValue
        {
            get
            {
                return _enteredValue;
            }
            set
            {
                OnPropertyChanged(ref _enteredValue, value, () => EnteredValue);
            }
        }

        private Tag EditedTag { get; set; }

        #endregion Properties

        #region Commands

        private ICommand _invertSearchTypeCommand;
        public ICommand InvertSearchTypeCommand
        {
            get
            {
                return _invertSearchTypeCommand ?? (_invertSearchTypeCommand = new RelayCommand(InvertSearchType));
            }
        }

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

        private ICommand _enterValueOkCommand;
        public ICommand EnterValueOkCommand
        {
            get
            {
                return _enterValueOkCommand ?? (_enterValueOkCommand = new RelayCommand(EnterValueOk));
            }
        }

        #endregion Commands

        #region Methods

        private async void SearchTagsAsync(string searchString)
        {
            HintBoxTags.Clear();

            try
            {
                var tags = await SearchTagsAsyncTask(searchString);

                if (!ValueEnterMode)
                {
                    HintBoxTags.SortList(tags);
                }
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Error while searching tags", ex.Message);
                Debug.WriteLine("Error while searching tags: " + ex.Message);
            }
        }

        private static Task<List<Tag>> SearchTagsAsyncTask(string searchString)
        {
            return Task.Run(() =>
                            {
                                return ImoutoService.Use(imoutoService =>
                                                         {
                                                             return imoutoService.SearchTags(searchString);
                                                         });
                            });
        }

        private void SelectTag(object param)
        {
            var tag = param as Tag;

            if (tag == null)
            {
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
                SelectedBindedTags.Add(new BindedTagVM(new BindedTag
                                                       {
                                                           Tag = tag,
                                                           Value =
                                                               (tag.HasValue && !string.IsNullOrWhiteSpace(EnteredValue))
                                                                   ? SelectedComparator + EnteredValue
                                                                   : null,
                                                           SearchType = SearchType.Include
                                                       }));
            }

            SearchString = string.Empty;
            OnSelectedTagsUpdated();
        }

        private void UnselectTag(object param)
        {
            var tag = param as BindedTagVM;

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
            EnteredValue = String.Empty;
        }

        private void EnterValueOk(object obj)
        {
            SelectTag(EditedTag);
        }

        private void InvertSearchType(object param)
        {
            var tag = param as BindedTagVM;

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

        #endregion Methods

        #region Events

        public event EventHandler SelectedTagsUpdated;

        private void OnSelectedTagsUpdated()
        {
            var handler = SelectedTagsUpdated;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events
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
            return Enum.GetValues(typeof (T)).Cast<T>();
        }
    }
}