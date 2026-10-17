using System.IO;

namespace ImoutoViewer.Extensions;

public static class FileSystemInfoExtensions
{
    public static string GetAlphanumericName(this FileSystemInfo fileSystemInfo)
    {
        var name = fileSystemInfo.Name;
        return name.GetAlphanumericName();
    }
}
