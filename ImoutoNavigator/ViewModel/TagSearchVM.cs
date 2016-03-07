using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Imouto.Navigator.Commands;
using Imouto.Navigator.WCF;
using Imouto.Utils;
using Imouto.WCFExchageLibrary.Data;

namespace Imouto.Navigator.ViewModel
{
    class TagSearchVM : VMBase
    {
        #region Fields

        private string _searchString;
        private Tag _selectedHintBoxTag;
        private bool _isValueEnterMode;
        private string _enteredValue;
        private List<string> _comparators;
        private string _selectedComparator;
        private KeyValuePair<string, int?> _selectedColleciton;

        private ICommand _enterValueOkCommand;
        private ICommand _unselectTagCommand;
        private ICommand _selectTagCommand;
        private ICommand _invertSearchTypeCommand;
        private ICommand _selectBindedTag;
        private int _rate;
        private int? _lastListEntryId = null;
        private bool _isRateSetted;

        #endregion Fields

        #region Constructors

        public TagSearchVM(ObservableCollection<CollectionVM> collections)
        {
            Collections.Add(new KeyValuePair<string, int?>("All", null));
            collections.ForEach(x => Collections.Add(new KeyValuePair<string, int?>(x.Name, x.Id)));
            SelectedColleciton = Collections.FirstOrDefault();

            ResetValueEnter();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TagSourceVM> CurrentTagsSources { get; } = new ObservableCollection<TagSourceVM>();

        public ObservableCollection<KeyValuePair<string, int?>> Collections { get; } = new ObservableCollection<KeyValuePair<string, int?>>();

        public KeyValuePair<string, int?> SelectedColleciton
        {
            get
            {
                return _selectedColleciton;
            }
            set
            {
                OnPropertyChanged(ref _selectedColleciton, value, () => SelectedColleciton);
                OnSelectedCollectionCahnged();
            }
        }

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

                if (string.IsNullOrWhiteSpace(value) || ValueEnterMode)
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
                return _comparators ?? (_comparators = ComparatorExtensions.GetValues<Comparator>()
                                                                           .Select(x => x.ToFriendlyString())
                                                                           .ToList());
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

        public int Rate
        {
            get
            {
                return _rate;
            }
            set
            {
                OnPropertyChanged(ref _rate, value, () => Rate);

                if (_lastListEntryId != null)
                {
                    SetRate(value, _lastListEntryId.Value);
                }
            }
        }

        public bool IsRateSetted
        {
            get
            {
                return _isRateSetted;
            }
            set
            {
                OnPropertyChanged(ref _isRateSetted, value, () => IsRateSetted);
            }
        }

        #endregion Properties

        #region Commands

        public ICommand InvertSearchTypeCommand
        {
            get
            {
                return _invertSearchTypeCommand ?? (_invertSearchTypeCommand = new RelayCommand(InvertSearchType));
            }
        }

        public ICommand SelectTagCommand
        {
            get
            {
                return _selectTagCommand ?? (_selectTagCommand = new RelayCommand(SelectTag));
            }
        }

        public ICommand UnselectTagCommand
        {
            get
            {
                return _unselectTagCommand ?? (_unselectTagCommand = new RelayCommand(UnselectTag));
            }
        }

        public ICommand EnterValueOkCommand
        {
            get
            {
                return _enterValueOkCommand ?? (_enterValueOkCommand = new RelayCommand(EnterValueOk));
            }
        }

        public ICommand SelectBindedTagCommand
        {
            get
            {
                return _selectBindedTag ?? (_selectBindedTag = new RelayCommand(SelectBindedTag));
            }
        }

        #endregion Commands

        #region Public methods

        public async void UpdateCurrentTags(INavigatorListEntry listEntry)
        {
            if (listEntry?.DbId == null)
            {
                IsRateSetted = false;

                return;
            }

            var id = listEntry.DbId.Value;

            var tags = await Task.Run(() =>
            {
                return ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.GetImageTags(id);
                });
            });
            _lastListEntryId = id;

            var tagVmsCollection = tags.Where(x => x.Tag.Type.Title != "LocalMeta")
                                       .Select(x => new BindedTagVM(x, listEntry.DbId))
                                       .ToList();

            CurrentTagsSources.Clear();

            var userTags = tagVmsCollection.Where(x => x.Model.Source == Source.User)
                                           .ToList();
            if (userTags.Any())
            {
                CurrentTagsSources.Add(new TagSourceVM
                {
                    Title = "User",
                    Tags = new ObservableCollection<BindedTagVM>(userTags)
                });
            }

            var parsedSources = tagVmsCollection.Select(x => x.Model.Source)
                                                .Where(x => x != Source.User)
                                                .Distinct();

            foreach (var parsedSource in parsedSources)
            {
                CurrentTagsSources.Add(new TagSourceVM
                {
                    Title = parsedSource.ToString(),
                    Tags = new ObservableCollection<BindedTagVM>(tagVmsCollection.Where(x => x.Model.Source == parsedSource)
                                                                                 .OrderBy(x => x.TypePriority)
                                                                                 .ThenBy(x => x.Tag.Title))
                });
            }

            GetRate(tags);
        }

        #endregion Public methods

        #region Private Methods

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
                    Value = (tag.HasValue && !string.IsNullOrWhiteSpace(EnteredValue))
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
            EnteredValue = string.Empty;
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

        private void SelectBindedTag(object param)
        {
            var tag = param as BindedTagVM;

            if (tag == null)
            {
                return;
            }

            if (SelectedBindedTags.All(x => x.Tag.Id != tag.Tag.Id || x.Value != tag.Value))
            {
                SelectedBindedTags.Add(new BindedTagVM(new BindedTag
                {
                    Tag = tag.Tag,
                    Value = tag.Value,
                    SearchType = SearchType.Include
                }));
            }

            OnSelectedTagsUpdated();
        }
        
        private void GetRate(List<BindedTag> tags)
        {
            var rateTag = tags.FirstOrDefault(x => x.Tag.Title == "Rate" && x.Tag.HasValue);
            if (rateTag != null)
            {
                try
                {
                    _rate = Int32.Parse(rateTag.Value);
                }
                catch (Exception)
                {
                    _rate = 0;
                }
            }
            else
            {
                _rate = 0;
            }

            IsRateSetted = true;
            OnPropertyChanged(() => Rate);
        }

        private async Task SetRate(int value, int? target)
        {
            var rateTag = await Task.Run(() =>
            {
                return ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.SearchTags("Rate", 1)
                                        .FirstOrDefault();
                });
            });

            if (rateTag == null)
            {
                await Task.Run(() =>
                {
                    ImoutoService.Use(imoutoService =>
                    {
                        var types = imoutoService.GetTagTypes();
                        var type = types.First(x => x.Title == "LocalMeta");

                        imoutoService.CreateTag(new Tag { Title = "Rate", HasValue = true, Type = type });
                    });
                });

                rateTag = await Task.Run(() =>
                {
                    return ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.SearchTags("Rate", 1).FirstOrDefault();
                    });
                });
            }

            ImoutoService.Use(imoutoService =>
            {
                imoutoService.BindTag(target.Value, new BindedTag() { Source = Source.User, Tag = rateTag, DateAdded = DateTime.Now, Value = value.ToString() });
            });
        }

        #endregion Private Methods

        #region Events

        public event EventHandler SelectedTagsUpdated;

        private void OnSelectedTagsUpdated()
        {
            var handler = SelectedTagsUpdated;
            handler?.Invoke(this, new EventArgs());
        }

        public event EventHandler SelectedCollectionCahnged;

        private void OnSelectedCollectionCahnged()
        {
            var handler = SelectedCollectionCahnged;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events
    }

    enum Comparator
    {
        Equal,
        NotEqual
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
            return Enum.GetValues(typeof (T))
                       .Cast<T>();
        }
    }
}