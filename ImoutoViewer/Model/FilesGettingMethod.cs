using System;

namespace ImoutoViewer.Model
{
    [Flags]
    public enum FilesGettingMethod
    {
        None = 0,
        Folder = 1,
        Subfolders = 2,
        AllDepthSubfolders = 4,
        Prefolders = 16,
        AllDepthPrefolder = 32,
        All = Folder | AllDepthSubfolders | AllDepthPrefolder
    }
}