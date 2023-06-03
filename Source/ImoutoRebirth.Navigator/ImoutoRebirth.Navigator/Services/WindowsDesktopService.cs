using System.Runtime.InteropServices;

namespace ImoutoRebirth.Navigator.Services;

public static partial class WindowsDesktopService
{
    [LibraryImport(
        "user32.dll",
        EntryPoint = "SystemParametersInfoW",
        StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private const int SpiSetDeskWallpaper = 20;
    private const int SpiFUpdateIniFile = 0x1;
    private const int SpiFSendChange = 0x2;

    public static void SetWallpaper(string pathToImage)
        => SystemParametersInfo(SpiSetDeskWallpaper, 0, pathToImage, SpiFUpdateIniFile | SpiFSendChange);
}
