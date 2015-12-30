using Imouto.Viewer.Commands;
using System.Windows.Input;
using System.Windows.Media;
using Imouto.Utils;
using Imouto.WCFExchageLibrary.Data;

namespace Imouto.Viewer.ViewModel
{
    class BindedTagVM : VMBase
    {
        private TagsVM _parantVM;
        private BindedTag _modelTag;
        private Brush _typeBrush;
        private ICommand _unbindCommand;

        public BindedTagVM(BindedTag tag, TagsVM tagsVM)
        {
            _modelTag = tag;
            _parantVM = tagsVM;
        }

        public string Title
        {
            get
            {
                var tag = _modelTag.Tag.Title;

                if (_modelTag.Tag.HasValue)
                {
                    return tag + " : " + _modelTag.Value;
                }
                else
                {
                    return tag;
                }
            }
        }

        public int Id
        {
            get
            {
                return _modelTag.Tag.Id.Value;
            }
        }

        public Brush TypeBrush
        {
            get
            {
                return _typeBrush ?? (_typeBrush = new SolidColorBrush(this._modelTag.Tag.Type.Color.ToColor()));
            }
        }

        public string Source
        {
            get
            {
                return _modelTag.Source.ToString();
            }
        }

        public bool IsEditable
        {
            get
            {
                return _modelTag.IsEditable;
            }
        }

        public int Count
        {
            get
            {
                return _modelTag.Tag.Count;
            }
        }

        public ICommand UnbindCommand
        {
            get
            {
                return _unbindCommand ?? (_unbindCommand = new RelayCommand(Unbind));
            }
        }

        private void Unbind(object obj)
        {
            _parantVM.UnbindTagAsync(this);
        }
    }
}
