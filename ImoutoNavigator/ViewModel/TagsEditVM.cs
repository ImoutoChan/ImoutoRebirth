using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoNavigator.Behavior;
using ImoutoNavigator.Commands;
using ImoutoNavigator.WCF;
using Imouto.WCFExchageLibrary.Data;

namespace ImoutoNavigator.ViewModel
{
    class TagsEditVM : VMBase, IDropable
    {
        #region Fields

        private string _searchText;
        private ICommand _createTagCommand;
        private ICommand _addTagsCommand;
        private ICommand _removeTagsCommand;
        private ICommand _saveCommand;
        private readonly MainWindowVM _parentVM;
        private CreateTagVM _createTagVM;
        private bool _isSavind;
        private bool _isSuccess;

        #endregion Fields

        #region Constructor

        public TagsEditVM(MainWindowVM parentVM)
        {
            parentVM.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedItems")
                {
                    OnPropertyChanged(() => SelectedItems);
                }
            };
            _parentVM = parentVM;
        }

        #endregion Constructor

        #region Properties

        #region Collections

        public ObservableCollection<BindedTagVM> FoundTags { get; } = new ObservableCollection<BindedTagVM>();

        public ObservableCollection<BindedTagVM> SelectedTags { get; } = new ObservableCollection<BindedTagVM>();

        #endregion Collections

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                OnPropertyChanged(ref _searchText, value, () => SearchText);
                SearchTagsAsync();
            }
        }

        public IEnumerable<INavigatorListEntry> SelectedItems => _parentVM.SelectedItems;

        public CreateTagVM CreateTagVM
        {
            get
            {
                return _createTagVM;
            }
            private set
            {
                OnPropertyChanged(ref _createTagVM, value, () => CreateTagVM);
            }
        }

        public bool IsSavind
        {
            get
            {
                return _isSavind;
            }
            set
            {
                _isSavind = value;
                OnPropertyChanged(() => IsSavind);
            }
        }

        public bool IsSuccess
        {
            get
            {
                return _isSuccess;
            }
            set
            {
                _isSuccess = value;
                OnPropertyChanged(() => IsSuccess);
            }
        }

        #endregion Properties

        #region Commands

        public ICommand CreateTagCommand => _createTagCommand ?? (_createTagCommand = new RelayCommand(CreateTag));

        private void CreateTag(object obj)
        {
            CreateTagVM = new CreateTagVM();
            CreateTagVM.RequestClosing += (sender, args) =>
            {
                CreateTagVM = null;
            };
        }

        public ICommand AddTagsCommand => _addTagsCommand ?? (_addTagsCommand = new RelayCommand(AddTags, CanAddTags));

        private bool CanAddTags(object obj)
        {
            return (obj as IList)?.Cast<BindedTagVM>().Any() ?? false;
        }

        private void AddTags(object obj)
        {
            var bindedTags = (obj as IList)?.Cast<BindedTagVM>();
            if (bindedTags == null
                || !bindedTags.Any())
            {
                return;
            }

            foreach (var bindedTag in bindedTags)
            {
                if (SelectedTags.Any(x => x.Tag.Id == bindedTag.Tag.Id && x.Value == bindedTag.Value))
                {
                    continue;
                }

                SelectedTags.Add(bindedTag);
            }
        }

        public ICommand RemoveTagsCommand
            => _removeTagsCommand ?? (_removeTagsCommand = new RelayCommand(RemoveTags, CanRemoveTags));

        private bool CanRemoveTags(object obj)
        {
            return (obj as IList)?.Cast<BindedTagVM>().Any() ?? false;
        }

        private void RemoveTags(object obj)
        {
            var bindedTags = (obj as IList)?.Cast<BindedTagVM>().ToList();
            if (bindedTags == null
                || !bindedTags.Any())
            {
                return;
            }

            foreach (var bindedTag in bindedTags)
            {
                var tagToRemove =
                    SelectedTags.FirstOrDefault(x => x.Tag.Id == bindedTag.Tag.Id && x.Value == bindedTag.Value);
                if (tagToRemove != null)
                {
                    SelectedTags.Remove(tagToRemove);
                }
            }
        }

        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(Save, CanSave));

        private bool CanSave(object obj)
        {
            return SelectedItems.Any() && SelectedTags.Any();
        }

        private async void Save(object obj)
        {
            var images = SelectedItems;
            var tags = SelectedTags;

            try
            {
                IsSavind = true;
                IsSuccess = false;
                await Task.Run(() =>
                {
                    ImoutoService.Use(imoutoService =>
                    {
                        imoutoService.BatchBindTag(
                                                   images.Where(x => x.DbId.HasValue).Select(x => x.DbId.Value).ToList(),
                                                   tags.Select(x => new BindedTag
                                                   {
                                                       Tag = x.Tag,
                                                       DateAdded = DateTime.Now,
                                                       Source = Source.User,
                                                       Value = (x.Tag.HasValue) ? x.Value : null
                                                   }).ToList());
                    });
                });
                IsSavind = false;

                IsSuccess = true;
                await Task.Delay(500);
                IsSuccess = false;
            }
            catch
            {
                IsSavind = false;
            }
        }

        #endregion Commands

        #region Methods

        private async void SearchTagsAsync()
        {
            string searchPattern;
            lock (SearchText)
            {
                searchPattern = SearchText;
            }
            try
            {
                var tags = await LoadTagsTask(searchPattern);

                lock (SearchText)
                {
                    if (SearchText == searchPattern)
                    {
                        ReloadFoundTags(tags);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Tags load error: " + ex.Message);
            }
        }

        private void ReloadFoundTags(List<Tag> tags)
        {
            FoundTags.Clear();
            foreach (var tag in tags)
            {
                FoundTags.Add(new BindedTagVM(new BindedTag
                {
                    Tag = tag
                }));
            }
        }

        private Task<List<Tag>> LoadTagsTask(string searchPattern)
        {
            return Task.Run(() =>
            {
                var tags = ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.SearchTags(searchPattern);
                });

                return tags;
            });
        }

        #endregion Methods

        #region IDpopable members

        public Type DataType => typeof (List<BindedTagVM>);

        public void Drop(object data, int index = -1)
        {
            var bindedTags = (data as List<BindedTagVM>)?.ToList();
            if (bindedTags == null
                || !bindedTags.Any())
            {
                return;
            }

            foreach (var bindedTag in bindedTags)
            {
                if (SelectedTags.Any(x => x.Tag.Id == bindedTag.Tag.Id && x.Value == bindedTag.Value))
                {
                    continue;
                }

                SelectedTags.Add(bindedTag);
            }
        }

        #endregion IDpopablemembers
    }
}