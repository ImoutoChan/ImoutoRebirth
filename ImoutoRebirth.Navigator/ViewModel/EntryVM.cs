﻿using System;
using System.Windows;

namespace ImoutoRebirth.Navigator.ViewModel
{
    static class EntryVM
    {
        public static INavigatorListEntry GetListEntry(string path, Size initPreviewSize, Guid? dbId = null)
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