using System.Collections.ObjectModel;
using ImoutoRebirth.Common.WPF;

namespace ImoutoViewer.ViewModel;

internal class SourceVM : VMBase
{
    public SourceVM(string title) => Title = title;

    public string Title { get; }

    public ObservableCollection<BindedTagVM> TagsCollection { get; } = new ObservableCollection<BindedTagVM>();
}
