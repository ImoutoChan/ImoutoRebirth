﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Imouto.WcfExchangeLibrary.Core.Data;
using ImoutoRebirth.Navigator.Behavior;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Tag;

namespace ImoutoRebirth.Navigator.ViewModel
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
        private bool _isSaving;
        private bool _isSuccess;
        private ICommand _setTagInfoContextCommand;
        private BindedTagVM _tagInfoContext;
        private readonly IFileTagService _fileTagService;
        private ITagService _tagService;

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

            _fileTagService = ServiceLocator.GetService<IFileTagService>();
            _tagService = ServiceLocator.GetService<ITagService>();
        }

        #endregion Constructor

        #region Properties

        #region Collections

        public ObservableCollection<BindedTagVM> FoundTags { get; } = new ObservableCollection<BindedTagVM>();

        public ObservableCollection<BindedTagVM> SelectedTags { get; } = new ObservableCollection<BindedTagVM>();

        public ObservableCollection<BindedTagVM> RecentlyTags { get; } = new ObservableCollection<BindedTagVM>();

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

        public BindedTagVM TagInfoContext
        {
            get
            {
                return _tagInfoContext;
            }
            set
            {
                OnPropertyChanged(ref _tagInfoContext, value, () => TagInfoContext);
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

        public bool IsSaving
        {
            get
            {
                return _isSaving;
            }
            set
            {
                _isSaving = value;
                OnPropertyChanged(() => IsSaving);
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

        public ICommand CreateTagCommand => _createTagCommand ??= new RelayCommand(CreateTag);

        private void CreateTag(object obj)
        {
            CreateTagVM = new CreateTagVM();
            CreateTagVM.RequestClosing += (sender, args) =>
            {
                CreateTagVM = null;
            };
        }

        public ICommand AddTagsCommand => _addTagsCommand ??= new RelayCommand(AddTags, CanAddTags);

        private bool CanAddTags(object obj)
        {
            return (obj as IList)?.Cast<BindedTagVM>()
                                  .Any() ?? obj is BindedTagVM;
        }

        private void AddTags(object obj)
        {
            var bindedTags = (obj as IList)?.Cast<BindedTagVM>();

            if (bindedTags == null)
            {
                var bindedTag = obj as BindedTagVM;

                if (bindedTag == null)
                {
                    return;
                }

                bindedTags = new List<BindedTagVM>
                {
                    bindedTag
                };
            }

            var bindedTagVms = bindedTags as IList<BindedTagVM> ?? bindedTags.ToList();
            if (!bindedTagVms.Any())
            {
                return;
            }

            foreach (var bindedTag in bindedTagVms)
            {
                if (SelectedTags.Any(x => x.Tag.Id == bindedTag.Tag.Id && x.Value == bindedTag.Value))
                {
                    continue;
                }

                SelectedTags.Add(bindedTag);
            }
        }

        public ICommand RemoveTagsCommand => _removeTagsCommand ??= new RelayCommand(RemoveTags, CanRemoveTags);

        private bool CanRemoveTags(object obj)
        {
            return (obj as IList)?.Cast<BindedTagVM>()
                                  .Any() ?? obj is BindedTagVM;
        }

        private void RemoveTags(object obj)
        {
            var bindedTags = (obj as IList)?.Cast<BindedTagVM>();

            if (bindedTags == null)
            {
                var bindedTag = obj as BindedTagVM;

                if (bindedTag == null)
                {
                    return;
                }

                bindedTags = new List<BindedTagVM>
                {
                    bindedTag
                };
            }

            var bindedTagVms = bindedTags as IList<BindedTagVM> ?? bindedTags.ToList();

            foreach (var bindedTag in bindedTagVms)
            {
                var tagToRemove = SelectedTags.FirstOrDefault(x => x.Tag.Id == bindedTag.Tag.Id && x.Value == bindedTag.Value);
                if (tagToRemove != null)
                {
                    SelectedTags.Remove(tagToRemove);
                }
            }
        }

        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

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
                IsSaving = true;
                IsSuccess = false;

                var imageIds = images
                    .Where(x => x.DbId.HasValue)
                    .Select(x => x.DbId.Value);

                var fileTags =
                    from imageId in imageIds
                    from tag in tags
                    select new FileTag(
                        imageId,
                        tag.Tag,
                        tag.Tag.HasValue
                            ? tag.Value
                            : null,
                        FileTagSource.Manual);

                await _fileTagService.BindTags(fileTags.ToArray());

                IsSaving = false;

                UpdateRecentlyTags(SelectedTags);

                IsSuccess = true;
                await Task.Delay(500);
                IsSuccess = false;
            }
            catch
            {
                IsSaving = false;
            }
        }

        private void UpdateRecentlyTags(IEnumerable<BindedTagVM> selectedTags)
        {
            foreach (var selectedTag in selectedTags)
            {
                var element = RecentlyTags.FirstOrDefault(x => x.Tag.Id == selectedTag.Tag.Id && x.Value == selectedTag.Value);
                if (element == null)
                {
                    RecentlyTags.Insert(0, selectedTag);
                }
                else
                {
                    RecentlyTags.Move(RecentlyTags.IndexOf(element), 0);
                }

                if (RecentlyTags.Count > 10)
                {
                    RecentlyTags.Remove(RecentlyTags.Last());
                }
            }
        }

        public ICommand SetTagInfoContextCommand => _setTagInfoContextCommand ??= new RelayCommand(SetTagInfoContext);

        private void SetTagInfoContext(object obj)
        {
            var bindedTag = obj as BindedTagVM;

            if (bindedTag == null)
            {
                TagInfoContext = null;
                return;
            }

            TagInfoContext = bindedTag;
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

        private void ReloadFoundTags(IReadOnlyCollection<Tag> tags)
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

        private Task<IReadOnlyCollection<Tag>> LoadTagsTask(string searchPattern)
        {
            return _tagService.SearchTags(searchPattern, 10);
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