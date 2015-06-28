using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoNavigator.Commands;
using ImoutoNavigator.WCF;
using WCFExchageLibrary.Data;

namespace ImoutoNavigator.ViewModel
{
    class CreateTagVM : VMBase
    {
        #region Fields

        private bool _tagTypesLoaded;
        private bool _isSavind;
        private bool _isSuccess;
        private string _title;
        private TagType _selectedType;
        private string _synonyms;
        private bool _hasValue;
        private ICommand _saveCommand;

        #endregion Fields

        #region Constructors

        public CreateTagVM()
        {
            ReloadTagTypesAsync();
        }

        #endregion Constructors

        #region Properties

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

        public ObservableCollection<TagType> TagTypes { get; } = new ObservableCollection<TagType>();

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

        /// <summary>
        ///     Separator :.:
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
                return _synonyms.Split(new[]
                {
                    ":.:"
                },
                                       StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

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

        #region Save command

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(Save, CanSave));
            }
        }

        private bool CanSave(object obj)
        {
            return SelectedType != null && !string.IsNullOrWhiteSpace(Title);
        }

        private async void Save(object obj)
        {
            try
            {
                IsSavind = true;
                await CreateTagTask(this);
                IsSavind = false;
                IsSuccess = true;
                await Task.Delay(500);
                OnRequestClosing();

                Debug.WriteLine("Tag creating", "Tag succsessfully created");
            }
            catch (Exception ex)
            {
                IsSavind = false;

                Debug.WriteLine("Tag creating", "Error while creating: " + ex.Message);
            }
        }

        #endregion Save command

        #region Cancel command

        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel));
            }
        }

        private void Cancel(object obj)
        {
            OnRequestClosing();
        }

        #endregion Cancel command

        #endregion Commands

        #region Methods

        private static readonly SemaphoreSlim ReloadTagTypesAsyncSemaphore = new SemaphoreSlim(1, 1);

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
            Debug.WriteLine("Tag creating", "Unable to load TagTypes. Creating process terminated");
        }

        private Task<List<TagType>> ReloadTagTypesTask()
        {
            return Task.Run(() =>
            {
                return ImoutoService.Use(imoutoService =>
                {
                    return imoutoService.GetTagTypes();
                });
            });
        }

        private Task CreateTagTask(CreateTagVM createTagVM)
        {
            return Task.Run(() =>
            {
                ImoutoService.Use(imoutoService =>
                {
                    imoutoService.CreateTag(new Tag
                    {
                        HasValue = createTagVM.HasValue,
                        SynonymsCollection = createTagVM.SynonymsCollection,
                        Title = createTagVM.Title,
                        Type = createTagVM.SelectedType
                    });
                });
            });
        }

        #endregion Methods

        #region Events

        public event EventHandler RequestClosing;

        private void OnRequestClosing()
        {
            var handler = RequestClosing;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events
    }
}
