using System;
using System.IO;
using System.Runtime.InteropServices;

internal static class VlcPluginCache
{
    public static FileInfo Generate(string libraryDirectory)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("Generating the published VLC cache requires Windows x64.");

        libraryDirectory = Path.GetFullPath(libraryDirectory);
        var cache = new FileInfo(Path.Combine(libraryDirectory, "plugins", "plugins.dat"));
        if (!Directory.Exists(cache.DirectoryName))
            throw new DirectoryNotFoundException(cache.DirectoryName);

        var core = NativeLibrary.Load(Path.Combine(libraryDirectory, "libvlccore.dll"));
        try
        {
            var library = NativeLibrary.Load(Path.Combine(libraryDirectory, "libvlc.dll"));
            try
            {
                var create = Marshal.GetDelegateForFunctionPointer<NewInstance>(
                    NativeLibrary.GetExport(library, "libvlc_new"));
                var release = Marshal.GetDelegateForFunctionPointer<ReleaseInstance>(
                    NativeLibrary.GetExport(library, "libvlc_release"));

                var previousPluginPath = Environment.GetEnvironmentVariable("VLC_PLUGIN_PATH");
                try
                {
                    Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", null);
                    cache.Delete();

                    string[] options = ["--quiet", "--reset-plugins-cache", "--"];
                    var instance = create(options.Length, options);
                    if (instance == IntPtr.Zero)
                        throw new InvalidOperationException("VLC failed to initialize while generating its plugin cache.");

                    release(instance);
                }
                finally
                {
                    Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", previousPluginPath);
                }
            }
            finally
            {
                NativeLibrary.Free(library);
            }
        }
        finally
        {
            NativeLibrary.Free(core);
        }

        cache.Refresh();
        if (!cache.Exists || cache.Length == 0)
            throw new InvalidOperationException($"VLC did not create a plugin cache at {cache.FullName}.");

        return cache;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr NewInstance(
        int argc,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 0)] string[] argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReleaseInstance(IntPtr instance);
}
