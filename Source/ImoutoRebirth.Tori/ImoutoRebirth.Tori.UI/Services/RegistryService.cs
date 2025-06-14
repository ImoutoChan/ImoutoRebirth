using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IRegistryService
{
    bool IsInstalled(out DirectoryInfo? installLocation);
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class RegistryService : IRegistryService
{
    private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\ImoutoRebirth";
    private const string InstallLocationKey = @"InstallLocation";

    public bool IsInstalled(out DirectoryInfo? installLocation)
    {
        installLocation = null;

        var location = Registry.GetValue(RegistryKeyPath, InstallLocationKey, null) as string;

        if (location == null)
            return false;

        installLocation = new DirectoryInfo(location);
        return installLocation.Exists;
    }
} 