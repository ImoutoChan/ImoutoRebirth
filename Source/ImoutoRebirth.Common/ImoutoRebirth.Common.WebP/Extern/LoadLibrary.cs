using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ImoutoRebirth.Common.WebP.Extern;

public static class LoadLibrary
{
    private static readonly object LockObj = new();
    private static readonly Dictionary<string, IntPtr> Loaded = new(StringComparer.OrdinalIgnoreCase);
    
    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, uint dwFlags);

    public static bool EnsureLoadedByPath(string fullPath, bool throwException)
    {
        //canonicalize as much as we can
        fullPath = Path.GetFullPath(fullPath);
        lock (LockObj)
        {
            if (Loaded.TryGetValue(fullPath, out var handle))
            {
                return true;
            }
            else
            {
                handle = LoadByPath(fullPath, throwException);
                if (handle != IntPtr.Zero)
                {
                    Loaded.Add(fullPath, handle);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Calls LoadLibraryEx with (Search DLL Load Dir and System32) flags. May increment reference count. Use EnsureLoadedByPath instead if you don't need a pointer.
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="throwException"></param>
    /// <returns></returns>
    public static IntPtr LoadByPath(string fullPath, bool throwException)
    {
        const uint loadLibrarySearchDllLoadDir = 0x00000100;
        const uint loadLibrarySearchSystem32 = 0x00000800;

        var moduleHandle = LoadLibraryEx(fullPath, IntPtr.Zero, loadLibrarySearchDllLoadDir | loadLibrarySearchSystem32);
        if (moduleHandle == IntPtr.Zero && throwException)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return moduleHandle;
    }


    public static void LoadWebPOrFail()
    {
        if (!AutoLoadNearby("libwebp.dll", true))
        {
            throw new FileNotFoundException("Failed to locate libwebp.dll");
        }
    }
    /// <summary>
    /// Looks for 'name' inside /x86/ or /x64/ (depending on arch) subfolders of known assembly locations
    /// </summary>
    /// <param name="name"></param>
    /// <param name="throwFailure"></param>
    /// <returns></returns>
    public static bool AutoLoadNearby(string name, bool throwFailure)
    {
        var a = Assembly.GetExecutingAssembly();
        return AutoLoad(name, new[]{Path.GetDirectoryName(a.Location), Path.GetDirectoryName(new Uri(a.Location).LocalPath)},throwFailure,throwFailure);
    }

    /// <summary>
    /// Looks for 'name' inside /x86/ and /x64/ subfolders of 'folder', depending on executing architecture. 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="searchFolders"></param>
    /// <param name="throwNotFound"></param>
    /// <param name="throwExceptions"></param>
    /// <returns></returns>
    public static bool AutoLoad(string name, string?[] searchFolders, bool throwNotFound, bool throwExceptions)
    {
        var searched = "";
        foreach (var folder in searchFolders)
        {
            if (folder == null)
                continue;
            
            var basePath = Path.Combine(folder, (IntPtr.Size == 8) ? "x64" : "x86");
            var fullPath = Path.Combine(basePath, name);
            if (string.IsNullOrEmpty(Path.GetExtension(fullPath)))
            {
                fullPath = fullPath + ".dll";
            }
            searched = searched + "\"" + fullPath + "\", ";
            if (File.Exists(fullPath))
            {
                if (EnsureLoadedByPath(fullPath, throwExceptions))
                {
                    return true;
                }
            }
        }
        if (throwNotFound)
        {
            throw new FileNotFoundException("Failed to locate '" + name + "' as " + searched.TrimEnd(' ', ','));
        }
        return false;
    }

}
