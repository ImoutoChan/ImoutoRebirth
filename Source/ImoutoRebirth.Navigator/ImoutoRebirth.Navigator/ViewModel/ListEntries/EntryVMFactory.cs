﻿using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal static class EntryVMFactory
{
    public static INavigatorListEntry? CreateListEntry(
        string path,
        Size initPreviewSize,
        FilesClient filesClient,
        Guid? dbId = null)
    {
        path = OverridePath(path);

        if (IsImage(path))
            return new ImageEntryVM(path, filesClient, initPreviewSize, dbId);

        if (!FileExists(path))
            return null;

        if (IsVideo(path))
            return new VideoEntryVM(path, filesClient, initPreviewSize, dbId);

        if (path.EndsWith(".zip"))
        {
            return new UgoiraEntryVM(path, filesClient, initPreviewSize, dbId);
        }

        if (path.EndsWith(".cbz")
            || path.EndsWith(".rar")
            || path.EndsWith(".cbr")
            || path.EndsWith(".7z")
            || path.EndsWith(".cb7"))
        {
            return new DodjiEntryVM(path, filesClient, initPreviewSize, dbId);
        }

        return null;
    }

    private static bool IsImage(string path)
    {
        string[] formats = { ".jpg", ".png", ".jpeg", ".bmp", ".gif", ".tiff", ".webp", ".jfif" };
        return formats.Any(item => path.EndsWith(item, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsVideo(string path) 
        => Enum.GetNames<VideoFormat>().Any(x => path.EndsWith('.' + x, StringComparison.OrdinalIgnoreCase)) 
           || path.EndsWith(".m4v") 
           || path.EndsWith(".swf");

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
