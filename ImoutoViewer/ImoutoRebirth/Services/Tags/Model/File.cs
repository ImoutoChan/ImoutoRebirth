using System;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

public class File
{
    public File(Guid id, string path)
    {
        Id = id;
        Path = path;
    }

    public string Path { get; }

    public Guid Id { get; }
}