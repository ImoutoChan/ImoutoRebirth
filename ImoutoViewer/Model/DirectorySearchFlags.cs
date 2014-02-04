using System;

namespace ImoutoViewer.Model
{
    [Serializable]
    [Flags]
    public enum DirectorySearchFlags
    {
        None = 0x0,
        Folder = 0x1,
        Subfolders = 0x2,
        AllDepthSubfolders = 0x4,
        Prefolders = 0x10,
        AllDepthPrefolder = 0x20
    }
}