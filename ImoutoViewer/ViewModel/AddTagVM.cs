using ImoutoViewer.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WCFExchageLibrary.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ImoutoViewer.WCF;
using System.Diagnostics;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace ImoutoViewer.ViewModel
{
    class AddTagVM : VMBase
    {
        private MainWindowVM _parent;

        public AddTagVM(MainWindowVM _mainWindow)
        {
            _parent = _mainWindow;
        }

        #region Properties

        private bool _isEnable;
        public bool IsEnabled
        {
            get
            {
                return _isEnable;
            }
            set
            {
                OnPropertyChanged(ref _isEnable, value, () => IsEnabled);
            }
        }

        private string _searchText = "";
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

        private ObservableCollection<Tag> _foundTags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> FoundTags
        {
            get
            {
                return _foundTags;
            }
        }

        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get
            {
                return _selectedTag;
            }
            set
            {
                OnPropertyChanged(ref _selectedTag, value, () => SelectedTag);
            }
        }

        private string _value = "";
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                OnPropertyChanged(ref _value, value, () => Value);
            }
        }

        #endregion Properties

        #region Commands

        #region Save command

        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(Save, CanSave));
            }
        }

        private bool CanSave(object obj)
        {
            return SelectedTag != null && (!SelectedTag.HasValue || !String.IsNullOrWhiteSpace(Value));
        }

        private void Save(object obj)
        {
            _parent.Tags.BindTagAsync(this);
        }

        #endregion Save command

        #region Reset command

        private ICommand _resetCommand;
        public ICommand ResetCommand
        {
            get
            {
                return _resetCommand ?? (_resetCommand = new RelayCommand(Reset));
            }
        }

        private void Reset(object obj)
        {
            SearchText = "";
            FoundTags.Clear();
            SelectedTag = null;
            Value = "";
            IsEnabled = true;
        }

        #endregion Reset command

        #endregion Commands

        #region Methods

        private async void SearchTagsAsync()
        {
            string searchPattern;
            lock (_searchText)
            {
                searchPattern = _searchText;
            }
            try
            {
                var tags = await LoadTagsTask(searchPattern);

                lock (_searchText)
                {
                    if (_searchText == searchPattern)
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
            var prevSelected = SelectedTag;
            FoundTags.Clear();

            foreach (var tag in tags)
            {
                FoundTags.Add(tag);
            }

            var newSelected = FoundTags.FirstOrDefault(x => x.Id == prevSelected.Id);
            if (newSelected != null)
            {
                SelectedTag = newSelected;
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

        private async void ShowMessageDialog(string message)
        {
            _parent.View.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;

            var mySettings = new MetroDialogSettings()
            {                
                AffirmativeButtonText = "Ok",
                ColorScheme = MetroDialogColorScheme.Accented
            };

            await _parent.View.ShowMessageAsync("Adding tag to current image", message, MessageDialogStyle.Affirmative, mySettings);
        }

        #endregion Methods
    }
}