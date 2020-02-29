using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using TagType = ImoutoRebirth.Navigator.Services.Tags.Model.TagType;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class CreateTagVM : VMBase
    {
        #region Fields

        private bool _tagTypesLoaded;
        private bool _isSaving;
        private bool _isSuccess;
        private string _title;
        private TagType _selectedType;
        private string _synonyms;
        private bool _hasValue;
        private ICommand _saveCommand;
        private readonly ITagService _tagService;

        #endregion Fields

        #region Constructors

        public CreateTagVM()
        {
            _tagService = ServiceLocator.GetService<ITagService>();
            ReloadTagTypesAsync();
        }

        #endregion Constructors

        #region Properties

        public TagType SelectedType
        {
            get => _selectedType;
            set
            {
                OnPropertyChanged(ref _selectedType, value, () => SelectedType);
            }
        }

        public ObservableCollection<TagType> TagTypes { get; } = new ObservableCollection<TagType>();

        public string Title
        {
            get => _title;
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
            get => _synonyms;
            set
            {
                OnPropertyChanged(ref _synonyms, value, () => Synonyms);
            }
        }

        public List<string> SynonymsCollection 
            => _synonyms?.Split(new[] { ":.:" }, StringSplitOptions.RemoveEmptyEntries).ToList() 
               ?? new List<string>();

        public bool HasValue
        {
            get => _hasValue;
            set
            {
                OnPropertyChanged(ref _hasValue, value, () => HasValue);
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged(() => IsSaving);
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                OnPropertyChanged(() => IsSuccess);
            }
        }

        #endregion Properties

        #region Commands

        #region Save command

        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

        private bool CanSave(object obj)
        {
            return SelectedType != null && !string.IsNullOrWhiteSpace(Title);
        }

        private async void Save(object obj)
        {
            try
            {
                IsSaving = true;
                await CreateTagTask(this);
                IsSaving = false;
                IsSuccess = true;
                await Task.Delay(500);
                OnRequestClosing();

                Debug.WriteLine("Tag creating", "Tag successfully created");
            }
            catch (Exception ex)
            {
                IsSaving = false;

                Debug.WriteLine("Tag creating", "Error while creating: " + ex.Message);
            }
        }

        #endregion Save command

        #region Cancel command

        private ICommand _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(Cancel);

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

        private static void TagTypesLoadFail(Exception e)
        {
            Debug.WriteLine("Tag creating", "Unable to load TagTypes. Creating process terminated");
        }

        private async Task<IReadOnlyCollection<TagType>> ReloadTagTypesTask()
        {
            return await _tagService.GеtTypes();
        }

        private async Task CreateTagTask(CreateTagVM createTagVm)
        {
            await _tagService.CreateTag(
                createTagVm.SelectedType.Id,
                createTagVm.Title,
                createTagVm.HasValue,
                createTagVm.SynonymsCollection);
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
