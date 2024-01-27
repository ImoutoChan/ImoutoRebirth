using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoViewer.Extensions;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ViewModel;

internal class BindedTagVM : VMBase
{
    private TagsVM _parantVM;
    private FileTag _modelTag;
    private Brush? _typeBrush;
    private ICommand? _unbindCommand;

    public BindedTagVM(FileTag tag, TagsVM tagsVM)
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

    public Guid Id => _modelTag.Tag.Id;

    public Brush TypeBrush => _typeBrush ?? (_typeBrush = new SolidColorBrush(ColorExtensions.ToColor(_modelTag.Tag.Type.Color)));

    public string Source => _modelTag.Source.ToString();

    public bool IsEditable => _modelTag.IsEditable;

    public int Count => _modelTag.Tag.Count;

    public ICommand UnbindCommand => _unbindCommand ?? (_unbindCommand = new RelayCommand(Unbind));

    private void Unbind(object? obj) => _parantVM.UnbindTagAsync(this);
}
