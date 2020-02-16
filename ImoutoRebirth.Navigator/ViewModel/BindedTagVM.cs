using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class BindedTagVM : VMBase
    {
        #region Static members

        private static readonly List<string> TypePriorities = new List<string>
        {
            "Artist",
            "Copyright",
            "Studio",
            "Circle",
            "Character",
            "Faults",
            "Medium",
            "Meta",
            "General"
        };

        #endregion Static members

        #region Fields

        private ICommand _unbindCommand;
        private readonly Guid? _targetId;
        private readonly IFileTagService _fileTagService;
        private SearchType _searchType;

        #endregion Fields

        #region Constructors

        public BindedTagVM(FileTag bindedTagModel, Guid? targetId = null)
        {
            Model = bindedTagModel;
            _fileTagService = ServiceLocator.GetService<IFileTagService>();
            _targetId = targetId;
        }

        #endregion Constructors

        #region Properties

        public SearchType SearchType
        {
            get => _searchType;
            set
            {
                _searchType = value;
                OnPropertyChanged(() => SearchType);
            }
        }

        public Tag Tag => Model.Tag;

        public Brush TypeBrush => new SolidColorBrush(Tag.Type.Color.ToColor());

        public string Synonyms => string.Join(", ", Tag.SynonymsCollection);

        public string Value
        {
            get => Model.Value;
            set
            {
                Model.Value = value;
                OnPropertyChanged(() => Value);
            }
        }

        public FileTag Model { get; }

        public string Title
        {
            get
            {
                var tag = Model.Tag.Title;

                if (Model.Tag.HasValue)
                {
                    return tag + " : " + Model.Value;
                }
                return tag;
            }
        }

        public bool IsEditable => Model.IsEditable;

        public int TypePriority
        {
            get
            {
                var priority = TypePriorities.IndexOf(Tag.Type.Title);
                return (priority >= 0) ? priority : 100;
            }
        }

        #endregion Properties

        #region Commands

        public ICommand UnbindCommand => _unbindCommand ??= new RelayCommand(UnbindAsync);

        private async void UnbindAsync(object obj)
        {
            if (_targetId == null
                || Model?.Tag?.Id == null)
            {
                return;
            }

            try
            {
                await UnbindTagTask(_targetId.Value, Model.Tag.Id.Value);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task UnbindTagTask(Guid imageId, Guid tagId)
        {
            await _fileTagService.UnbindTag(imageId, tagId);
        }

        #endregion Commands

        #region Methods

        public override string ToString()
        {
            return $"{Tag.Id} - {Tag.Title} : {Value}";
        }

        #endregion Methods
    }
}
