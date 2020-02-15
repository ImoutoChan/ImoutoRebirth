using System;
using System.Windows;
using System.Windows.Input;
using ImoutoRebirth.Navigator.Behavior;

namespace ImoutoRebirth.Navigator.ViewModel
{
    internal interface INavigatorListEntry : IDragable
    {
        void Load();

        void UpdatePreview(Size previewSize);

        ICommand OpenCommand { get; }

        ListEntryType Type { get; }

        Size ViewPortSize { get; }

        Guid? DbId { get; }

        string Path { get; }
    }
}