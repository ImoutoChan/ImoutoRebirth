namespace ImoutoRebirth.Common;

public static class DirectoryInfoExtensions
{
    public static string CombineToFilePath(this DirectoryInfo di, string filename)
        => Path.Combine(di.FullName, filename);

    public static DirectoryInfo CombineToDirectory(this DirectoryInfo di, string subdirectoryName) 
        => new(Path.Combine(di.FullName, subdirectoryName));

    public static FileInfo CombineToFileInfo(this DirectoryInfo di, string filename)
        => new(Path.Combine(di.FullName, filename));
}
