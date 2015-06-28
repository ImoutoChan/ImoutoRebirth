using System.Windows;
using System.Windows.Input;
using ImoutoNavigator.Behavior;

namespace ImoutoNavigator.ViewModel
{
    interface INavigatorListEntry : IDragable
    {
        void Load();

        void UpdatePreview(Size previewSize);

        ICommand OpenCommand { get; }

        ListEntryType Type { get; }

        Size ViewPortSize { get; }

        int? DbId { get; }
    }
}