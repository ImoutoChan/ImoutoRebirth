#nullable enable
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Imouto;
using ImoutoRebirth.LilinService.WebApi.Client;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal static class EntryVMFactory
{
    public static INavigatorListEntry? CreateListEntry(
        string path,
        Size initPreviewSize,
        FilesClient filesClient,
        Guid? dbId = null)
    {
        // todo disk replacement
        // path = "Q" + path.Substring(1);
        
        path = OverridePath(path);

        if (path.IsImage() || path.EndsWith(".webp") || path.EndsWith(".jfif"))
            return new ImageEntryVM(path, filesClient, initPreviewSize, dbId);

        if (!FileExists(path))
            return null;

        if (path.IsVideo() || path.EndsWith(".m4v"))
            return new VideoEntryVM(path, filesClient, initPreviewSize, dbId);

        if (path.EndsWith(".zip"))
            return new UgoiraEntryVM(path, filesClient, initPreviewSize, dbId);

        return null;
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
