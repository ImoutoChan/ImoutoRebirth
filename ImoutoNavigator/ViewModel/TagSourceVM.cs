using System.Collections.ObjectModel;

namespace Imouto.Navigator.ViewModel
{
    class TagSourceVM : VMBase
    {
        #region Properties

        public string Title { get; set; }

        public ObservableCollection<BindedTagVM> Tags { get; set; }

        #endregion Properties
    }
}