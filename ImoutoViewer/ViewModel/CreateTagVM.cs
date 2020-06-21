using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Imouto.WCFExchageLibrary.Data;
using ImoutoViewer.Commands;

namespace ImoutoViewer.ViewModel
{
    class CreateTagVM : VMBase
    {
        private MainWindowVM _parent;
        private bool _tagTypesLoaded = false;

        public CreateTagVM(MainWindowVM _parentVM)
        {
            _parent = _parentVM;
        }

        #region Properties

        private TagType _selectedType;

        public TagType SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                OnPropertyChanged(ref _selectedType, value, () => SelectedType);
            }
        }


        private ObservableCollection<TagType> _tagTypes = new ObservableCollection<TagType>();

        public ObservableCollection<TagType> TagTypes
        {
            get
            {
                return _tagTypes;
            }
        }


        private string _title;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                OnPropertyChanged(ref _title, value, () => Title);
            }
        }


        private string _synonyms;

        /// <summary>
        /// Separator :.:
        /// </summary>
        public string Synonyms
        {
            get
            {
                return _synonyms;
            }
            set
            {
                OnPropertyChanged(ref _synonyms, value, () => Synonyms);
            }
        }

        public List<string> SynonymsCollection
        {
            get
            {
                return _synonyms.Split(new[] { ":.:" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        private bool _hasValue;

        /// <summary>
        /// Separator :.:
        /// </summary>
        public bool HasValue
        {
            get
            {
                return _hasValue;
            }
            set
            {
                OnPropertyChanged(ref _hasValue, value, () => HasValue);
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
            return SelectedType != null && !String.IsNullOrWhiteSpace(Title);
        }

        private void Save(object obj)
        {
            _parent.Tags.CreateTagAsync(this);
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
            if (obj is bool && !(bool)obj)
            {
                return;
            }

            _tagTypesLoaded = false;
            ReloadTagTypesAsync();
            SelectedType = null;
            Synonyms = "";
            HasValue = false;
            Title = "";
        }


        #endregion Reset command

        #endregion Commands

        private static SemaphoreSlim ReloadTagTypesAsyncSemaphore = new SemaphoreSlim(1, 1);
        private async void ReloadTagTypesAsync()
        {
            await ReloadTagTypesAsyncSemaphore.WaitAsync();

            try
            {
                if (_tagTypesLoaded)
                {
                    return;
                }

                var tagTypes = await ReloadTagTypesTask();

                TagTypes.Clear();
                foreach (var tagType in tagTypes)
                {
                    TagTypes.Add(tagType);
                }

                _tagTypesLoaded = true;
            }
            catch (Exception e)
            {
                TagTypesLoadFail(e);
            }
            finally
            {
                ReloadTagTypesAsyncSemaphore.Release();
            }
        }

        private void TagTypesLoadFail(Exception e)
        {
            _parent.View.ShowMessageDialog("Tag creating", "Unable to load TagTypes. Creating process terminated");
            _parent.View.CloseAllFlyouts();
        }

        private Task<List<TagType>> ReloadTagTypesTask()
        {
            // TODO load tag types
            //return Task.Run(() =>
            //{
            //    return ImoutoService.Use(imoutoService =>
            //    {
            //        return imoutoService.GetTagTypes();
            //    });
            //});
            return Task.FromResult(Array.Empty<TagType>().ToList());
        }
    }
}
