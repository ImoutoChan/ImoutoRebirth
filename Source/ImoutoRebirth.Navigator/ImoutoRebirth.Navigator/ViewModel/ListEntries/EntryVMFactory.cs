using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal static class EntryVMFactory
{
    private static readonly string[] ImageExtensions = [".jpg", ".png", ".jpeg", ".bmp", ".gif", ".tiff", ".webp", ".jfif"];
    private static readonly string[] VideoExtensions = Enum.GetNames<VideoFormat>().Select(x => "." + x).ToArray();

    public static INavigatorListEntry? CreateListEntry(
        string path,
        Size initPreviewSize,
        Guid? dbId = null)
    {
        path = OverridePath(path);

        if (IsImage(path))
            return new ImageEntryVM(path, initPreviewSize, dbId);

        if (!FileExists(path))
            return null;

        if (IsVideo(path))
            return new VideoEntryVM(path, initPreviewSize, dbId);

        if (path.EndsWith(".zip"))
        {
            return new UgoiraEntryVM(path, initPreviewSize, dbId);
        }

        if (path.EndsWith(".cbz")
            || path.EndsWith(".rar")
            || path.EndsWith(".cbr")
            || path.EndsWith(".7z")
            || path.EndsWith(".cb7"))
        {
            return new DodjiEntryVM(path, initPreviewSize, dbId);
        }

        return null;
    }

    private static bool IsImage(string path) => HasExtension(path, ImageExtensions);

    private static bool IsVideo(string path)
        => HasExtension(path, VideoExtensions) || path.EndsWith(".m4v") || path.EndsWith(".swf");

    private static bool HasExtension(string path, string[] extensions)
    {
        foreach (var extension in extensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string OverridePath(string path)
    {
        var overrides = Settings.Default.PathOverrides;

        if (string.IsNullOrWhiteSpace(overrides)) 
            return path;
        
        var replaces = overrides.Split(";;;", StringSplitOptions.RemoveEmptyEntries);
        if (!replaces.Any()) 
            return path;
        
        return replaces
            .Select(replace => replace.Split("->", StringSplitOptions.RemoveEmptyEntries))
            .Where(split => split.Length == 2)
            .Aggregate(path, (current, split) => current.Replace(split[0], split[1]));
    }

    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool PathFileExists(StringBuilder path);

    private static bool FileExists(string path)
    {
        // A StringBuilder is required for interops calls that use strings
        var builder = new StringBuilder();
        builder.Append(path);
        return PathFileExists(builder);
    }
}
