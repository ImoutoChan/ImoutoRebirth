using System.Collections.ObjectModel;

namespace ImoutoViewer.ViewModel;

class SourceVM : VMBase
{
    public string Title { get; set; }

    public ObservableCollection<BindedTagVM> TagsCollection { get; } = new ObservableCollection<BindedTagVM>();
}