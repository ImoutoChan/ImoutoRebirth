using System.Windows;
using System.Windows.Input;
using Imouto.Navigator.Behavior;

namespace Imouto.Navigator.ViewModel
{
    interface INavigatorListEntry : IDragable
    {
        void Load();

        void UpdatePreview(Size previewSize);

        ICommand OpenCommand { get; }

        ListEntryType Type { get; }

        Size ViewPortSize { get; }

        int? DbId { get; }

        string Path { get; }
    }
}