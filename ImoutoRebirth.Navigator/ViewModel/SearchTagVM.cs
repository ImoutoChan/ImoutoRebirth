#nullable enable
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.ViewModel
{
    internal class SearchTagVM : VMBase
    {
        private readonly SearchTag _model;

        public SearchTagVM(SearchTag model)
        {
            _model = model;
        }

        public Tag Tag => _model.Tag;

        public string? Value
        {
            get { return _model.Value; }
            set
            {
                _model.Value = value;
                OnPropertyChanged(() => Value);
            }
        }

        public SearchType SearchType
        {
            get => _model.SearchType;
            set
            {
                _model.SearchType = value;
                OnPropertyChanged(() => SearchType);
            }
        }

        public SearchTag Model => _model;
    }
}