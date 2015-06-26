using System;
using System.Windows;

namespace ImoutoNavigator.ViewModel
{
    static class EntryVM
    {
        public static INavigatorListEntry GetListEntry(string path, Size initPreviewSize)
        {
            try
            {
                return new ImageEntryVM(path, initPreviewSize);
            }
            catch (ArgumentException)
            {
                try
                {
                    return new VideoEntryVM(path, initPreviewSize);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}