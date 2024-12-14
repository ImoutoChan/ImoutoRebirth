using System.IO;

namespace ImoutoRebirth.Navigator.Utils;

public static class DirectoryInfoExtensions
{
    public static DirectoryInfo EnsureExists(this DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
            directoryInfo.Create();
        return directoryInfo;
    }
}
