#nullable enable
using System;
using System.IO;
using System.Windows;
using Imouto;

namespace ImoutoRebirth.Navigator.ViewModel
{
    internal static class EntryVMFactory
    {
        public static INavigatorListEntry? CreateListEntry(
            string path, 
            Size initPreviewSize, 
            Guid? dbId = null)
        {
            // todo disk replacement
            path = "Q" + path.Substring(1);

            if (!new FileInfo(path).Exists)
                return null;

            if (path.IsImage())
                return new ImageEntryVM(path, initPreviewSize, dbId);

            if (path.IsVideo())
                return new VideoEntryVM(path, initPreviewSize, dbId);

            return null;
        }
    }
}