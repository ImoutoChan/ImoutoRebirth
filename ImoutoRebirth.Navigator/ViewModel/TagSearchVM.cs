using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;
using SearchType = ImoutoRebirth.Navigator.Services.Tags.Model.SearchType;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel
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
        private KeyValuePair<string, Guid?> _selectedColleciton;

        private ICommand _enterValueOkCommand;
        private ICommand _unselectTagCommand;
        private ICommand _selectTagCommand;
        private ICommand _invertSearchTypeCommand;
        private ICommand _selectBindedTag;
        private int _rate;
        private Guid? _lastListEntryId = null;
        private bool _isRateSetted;
        private bool _isFavorite;
        private readonly IFileTagService _fileTagService;
        private readonly ITagService _tagService;

        #endregion Fields

        #region Constructors

        public TagSearchVM(ObservableCollection<CollectionVM> collections)
        {
            _fileTagService = ServiceLocator.GetService<IFileTagService>();
            _tagService = ServiceLocator.GetService<ITagService>();

            Collections.Add(new KeyValuePair<string, Guid?>("All", null));

            foreach (var collectionVm in collections)
            {
                Collections.Add(new KeyValuePair<string, Guid?>(collectionVm.Name, collectionVm.Id));
            }

            SelectedColleciton = Collections.FirstOrDefault();

            ResetValueEnter();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TagSourceVM> CurrentTagsSources { get; } = new ObservableCollection<TagSourceVM>();

        public ObservableCollection<KeyValuePair<string, Guid?>> Collections { get; }
            = new ObservableCollection<KeyValuePair<string, Guid?>>();

        public KeyValuePair<string, Guid?> SelectedColleciton
        {
            get => _selectedColleciton;
            set
            {
                OnPropertyChanged(ref _selectedColleciton, value, () => SelectedColleciton);
                OnSelectedCollectionCahnged();
            }
        }

        public ObservableCollection<Tag> HintBoxTags { get; } = new ObservableCollection<Tag>();

        public Tag SelectedHintBoxTag
        {
            get => _selectedHintBoxTag;
            set
            {
                OnPropertyChanged(ref _selectedHintBoxTag, value, () => SelectedHintBoxTag);
            }
        }

        public ObservableCollection<SearchTagVM> SelectedBindedTags { get; } = new ObservableCollection<SearchTagVM>();

        public string SearchString
        {
            get => _searchString;
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
            get => _isValueEnterMode;
            set
            {
                OnPropertyChanged(ref _isValueEnterMode, value, () => ValueEnterMode);
            }
        }

        public List<string> Comparators
        {
            get
            {
                return _comparators ??= ComparatorExtensions.GetValues<Comparator>()
                    .Select(x => x.ToFriendlyString())
                    .ToList();
            }
        }

        public string SelectedComparator
        {
            get => _selectedComparator;
            set
            {
                OnPropertyChanged(ref _selectedComparator, value, () => SelectedComparator);
            }
        }

        public string EnteredValue
        {
            get => _enteredValue;
            set
            {
                OnPropertyChanged(ref _enteredValue, value, () => EnteredValue);
            }
        }

        private Tag EditedTag { get; set; }

        public int Rate
        {
            get => _rate;
            set
            {
                OnPropertyChanged(ref _rate, value, () => Rate);

                if (_lastListEntryId != null)
                {
                    SetRate(value, _lastListEntryId.Value);
                }
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                OnPropertyChanged(ref _isFavorite, value, () => IsFavorite);

                if (_lastListEntryId != null)
                {
                    SetFavorite(value, _lastListEntryId.Value);
                }
            }
        }

        public bool IsRateSetted
        {
            get => _isRateSetted;
            set
            {
                OnPropertyChanged(ref _isRateSetted, value, () => IsRateSetted);
            }
        }

        #endregion Properties

        #region Commands

        public ICommand InvertSearchTypeCommand => _invertSearchTypeCommand ??= new RelayCommand(InvertSearchType);

        public ICommand SelectTagCommand => _selectTagCommand ??= new RelayCommand(SelectTag);

        public ICommand UnselectTagCommand => _unselectTagCommand ??= new RelayCommand(UnselectTag);

        public ICommand EnterValueOkCommand => _enterValueOkCommand ??= new RelayCommand(EnterValueOk);

        public ICommand SelectBindedTagCommand => _selectBindedTag ??= new RelayCommand(SelectBindedTag);

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

            var tags = await _fileTagService.GetFileTags(id);

            _lastListEntryId = id;

            var tagVmsCollection = tags.Where(x => x.Tag.Type.Title != "LocalMeta")
                                       .Select(x => new BindedTagVM(x, listEntry.DbId))
                                       .ToList();

            CurrentTagsSources.Clear();

            var userTags = tagVmsCollection.Where(x => x.Model.Source == FileTagSource.Manual)
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
                                                .Where(x => x != FileTagSource.Manual)
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

            GetFavorite(tags);
            GetRate(tags);
            IsRateSetted = true;
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

        private Task<IReadOnlyCollection<Tag>> SearchTagsAsyncTask(string searchString)
        {
            return _tagService.SearchTags(searchString, 10);
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
                var value = (tag.HasValue && !string.IsNullOrWhiteSpace(EnteredValue))
                    ? SelectedComparator + EnteredValue
                    : null;

                SelectedBindedTags.Add(
                    new SearchTagVM(
                        new SearchTag(
                            tag,
                            value)));
            }

            SearchString = string.Empty;
            OnSelectedTagsUpdated();
        }

        private void UnselectTag(object param)
        {
            var tag = param as SearchTagVM;

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
            var tag = param as SearchTagVM;

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
                SelectedBindedTags.Add(new SearchTagVM(new SearchTag(tag.Tag, tag.Value, SearchType.Include)));
            }

            OnSelectedTagsUpdated();
        }
        
        private void GetRate(IReadOnlyCollection<FileTag> tags)
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

            OnPropertyChanged(() => Rate);
        }

        private async Task SetRate(int value, Guid fileId)
        {
            //var rateTag = await ImoutoService.UseAsync(imoutoService =>
            //{
            //    return imoutoService.SearchTags("Rate", 1)
            //                        .FirstOrDefault();
            //});

            //if (rateTag.Title != "Rate")
            //{
            //    rateTag = null;
            //}

            //if (rateTag == null)
            //{
            //    await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        var types = imoutoService.GetTagTypes();
            //        var type = types.First(x => x.Title == "LocalMeta");

            //        imoutoService.CreateTag(new Tag
            //        {
            //            Title = "Rate",
            //            HasValue = true,
            //            Type = type
            //        });
            //    });

            //    rateTag = await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        return imoutoService.SearchTags("Rate", 1)
            //                            .FirstOrDefault();
            //    });
            //}

            //var rateTag = await _tagService.GetOrCreateRateTag();

            //await ImoutoService.UseAsync(imoutoService =>
            //{
            //    imoutoService.BindTag(target, new BindedTag() { Source = Source.User, Tag = rateTag, DateAdded = DateTime.Now, Value = value.ToString() });
            //});

            //await _fileTagService.BindTag(target, rateTag, Source.User, value.ToString());

            await _fileTagService.SetRate(fileId, new Rate(value));
        }

        private void GetFavorite(IReadOnlyCollection<FileTag> tags)
        {
            var favTag = tags.FirstOrDefault(x => x.Tag.Title == "favorite");
            _isFavorite = favTag != null;
            OnPropertyChanged(() => IsFavorite);
        }

        private async Task SetFavorite(bool value, Guid fileId)
        {
            //var favTag = await ImoutoService.UseAsync(imoutoService =>
            //{
            //    return imoutoService.SearchTags("favorite", 1)
            //                        .FirstOrDefault();
            //});

            //if (favTag == null)
            //{
            //    await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        var types = imoutoService.GetTagTypes();
            //        var type = types.First(x => x.Title == "LocalMeta");

            //        imoutoService.CreateTag(new Tag { Title = "favorite", HasValue = false, Type = type });
            //    });

            //    favTag = await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        return imoutoService.SearchTags("favorite", 1).FirstOrDefault();
            //    });
            //}

            //var favTag = await _tagService.GetOrCreateFavTag();

            //var tags = await ImoutoService.UseAsync(imoutoService => imoutoService.GetImageTags(target));

            //var favBindedTag = tags.FirstOrDefault(x => x.Tag.Id == favTag.Id);

            //if (favBindedTag != null && !value)
            //{
            //    await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        imoutoService.UnbindTag(target, favTag.Id.Value);
            //    });
            //}
            //else if (favBindedTag == null && value)
            //{
            //    await ImoutoService.UseAsync(imoutoService =>
            //    {
            //        imoutoService.BindTag(target, new BindedTag { Source = Source.User, Tag = favTag, DateAdded = DateTime.Now });
            //    });
            //}

            await _fileTagService.SetFavorite(fileId, value);
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