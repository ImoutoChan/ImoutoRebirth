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
    class TagSearchVM : VMBase
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
            this._mainWindowVM = mainWindowVM;
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
                OnPropertyChanged(ref _selectedHintBoxTag, value, () => this.SelectedHintBoxTag);
            }
        }

        public ObservableCollection<BindedTag> SelectedBindedTags { get; } = new ObservableCollection<BindedTag>();

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

                OnPropertyChanged(ref _searchString, value, () => this.SearchString);
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
                OnPropertyChanged(ref _isValueEnterMode, value, () => this.ValueEnterMode);
            }
        }

        public List<string> Comparators
        {
            get
            {
                return _comparators ?? (_comparators = ComparatorExtensions.GetValues<Comparator>().Select(x => x.ToFriendlyString()).ToList());
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
                OnPropertyChanged(ref _selectedComparator, value, () => this.SelectedComparator);
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
                OnPropertyChanged(ref _enteredValue, value, () => this.EnteredValue);
            }
        }

        private Tag EditedTag { get; set; }

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
                    Sorts.SortList(HintBoxTags, tags);
                }
            }
            catch (Exception ex)
            {
                App.MainWindowVM?.SetStatusError("Error while searching tags", ex.Message);
                Debug.WriteLine("Error while searching tags: " + ex.Message);
            }
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

            if (tag.HasValue && !ValueEnterMode)
            {
                SearchString = tag.Title;
                ValueEnterMode = true;
                EditedTag = tag;
                return;
            }

            if (SelectedBindedTags.All(x => x.Tag.Id != tag.Id || x.Value != EnteredValue))
            {
                SelectedBindedTags.Add(new BindedTag { Tag = tag, Value = (tag.HasValue && !String.IsNullOrWhiteSpace(EnteredValue)) ? SelectedComparator + EnteredValue : null });
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

            var tag = param as BindedTag;

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

    enum Comparator
    {
        Equal,
        NotEqual,
        //MoreOrEqual,
        //More,
        //LessOrEqual,
        //Less
    }

    static class ComparatorExtensions
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
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
