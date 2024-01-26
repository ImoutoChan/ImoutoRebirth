using System.Collections.ObjectModel;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class TagSourceVM : VMBase
{
    public TagSourceVM(string title, ObservableCollection<BindedTagVM> tags)
    {
        Title = title;
        Tags = tags;
    }

    public string Title { get; private set; }

    public ObservableCollection<BindedTagVM> Tags { get; private set; }
}
