using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace ImoutoRebirth.Tori.Services;

public interface IRegistryService
{
    bool IsInstalled([NotNullWhen(returnValue: true)] out DirectoryInfo? installLocation);
    void SetInstalled(DirectoryInfo installLocation);
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class RegistryService : IRegistryService
{
    private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\ImoutoRebirth";
    private const string InstallLocationKey = @"InstallLocation";

    public bool IsInstalled([NotNullWhen(returnValue: true)] out DirectoryInfo? installLocation)
    {
        installLocation = null;

        var location = Registry.GetValue(RegistryKeyPath, InstallLocationKey, null) as string;

        if (location == null)
            return false;

        installLocation = new DirectoryInfo(location);
        return true;
    }

    public void SetInstalled(DirectoryInfo installLocation)
        => Registry.SetValue(RegistryKeyPath, InstallLocationKey, installLocation.FullName);
}