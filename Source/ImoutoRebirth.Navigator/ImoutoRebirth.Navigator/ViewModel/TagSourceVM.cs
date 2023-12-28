using System.Collections.ObjectModel;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class TagSourceVM : VMBase
{
    #region Properties

    public string Title { get; set; }

    public ObservableCollection<BindedTagVM> Tags { get; set; }

    #endregion Properties
}