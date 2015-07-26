using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Imouto.Navigator.Commands;
using Imouto.Navigator.WCF;
using Imouto.Utils;
using Imouto.WCFExchageLibrary.Data;

namespace Imouto.Navigator.ViewModel
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
        private readonly int? _targetId;

        #endregion Fields

        #region Constructors

        public BindedTagVM(BindedTag bindedTagModel, int? targetId = null)
        {
            Model = bindedTagModel;
            _targetId = targetId;
        }

        #endregion Constructors

        #region Properties

        public SearchType SearchType
        {
            get
            {
                return Model.SearchType;
            }
            set
            {
                Model.SearchType = value;
                OnPropertyChanged(() => SearchType);
            }
        }

        public Tag Tag => Model.Tag;

        public Brush TypeBrush => new SolidColorBrush(Tag.Type.Color.ToColor());

        public string Synonyms => string.Join(", ", Tag.SynonymsCollection);

        public string Value
        {
            get
            {
                return Model.Value;
            }
            set
            {
                Model.Value = value;
                OnPropertyChanged(() => Value);
            }
        }

        public BindedTag Model { get; }

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

        public bool IsEditable
        {
            get
            {
                return Model.IsEditable;
            }
        }

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

        public ICommand UnbindCommand
        {
            get
            {
                return _unbindCommand ?? (_unbindCommand = new RelayCommand(UnbindAsync));
            }
        }

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

        private Task UnbindTagTask(int imageId, int tagId)
        {
            return Task.Run(() =>
            {
                ImoutoService.Use(imoutoService =>
                {
                    imoutoService.UnbindTag(imageId, tagId);
                });
            });
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
