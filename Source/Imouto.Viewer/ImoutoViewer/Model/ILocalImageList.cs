using System.IO;

namespace ImoutoViewer.Model;

internal interface ILocalImageList : IDisposable
{
    LocalImage? CurrentImage { get; }

    int ImagesCount { get; }

    int? CurrentImageIndex { get; }

    DirectoryInfo? CurrentDirectory { get; }

    int DirectoriesCount { get; }

    int? CurrentDirectoryIndex { get; }

    bool IsEmpty { get; }

    bool IsDirectoryActive { get; }

    void Next();

    void Previous();

    void NextFolder();

    void PrevFolder();

    void ResortFiles();

    event EventHandler? CurrentImageChanged;
}
