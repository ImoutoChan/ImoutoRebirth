using System;
using System.Windows;

namespace Imouto.Navigator.ViewModel
{
    static class EntryVM
    {
        public static INavigatorListEntry GetListEntry(string path, Size initPreviewSize, int? dbId = null)
        {
            try
            {
                return new ImageEntryVM(path, initPreviewSize, dbId);
            }
            // TODO Rewrite this shit
            catch (ArgumentException)
            {
                try
                {
                    return new VideoEntryVM(path, initPreviewSize, dbId);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}