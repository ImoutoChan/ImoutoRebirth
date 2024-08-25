using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class TagSourceVM : ObservableObject
{
    public TagSourceVM(string title, ObservableCollection<BindedTagVM> tags)
    {
        Title = title;
        Tags = tags;
    }

    public string Title { get; private set; }

    public ObservableCollection<BindedTagVM> Tags { get; private set; }
}
