using System.Reflection;

namespace ImoutoRebirth.Tori.Services;

public interface IVersionService
{
    string GetNewVersion();

    Task<string> GetLocalVersion(DirectoryInfo installedLocation);

    Task SetLocalVersionAsNew(DirectoryInfo installedLocation);
}

public class VersionService : IVersionService
{
    private const string VersionFileName = "version.txt";

    public string GetNewVersion()
        => Assembly.GetEntryAssembly()
               ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
               ?.InformationalVersion
           ?? typeof(VersionService).Assembly.GetName().Version!.ToString();

    public async Task<string> GetLocalVersion(DirectoryInfo installedLocation)
    {
        if (!installedLocation.Exists)
            return "0.0.0";

        var versionFile = installedLocation.GetFiles().FirstOrDefault(x => x.Name == VersionFileName);

        if (versionFile?.Exists != true)
            return "0.0.0";

        return await File.ReadAllTextAsync(versionFile.FullName);
    }

    public async Task SetLocalVersionAsNew(DirectoryInfo installedLocation)
        => await File.WriteAllTextAsync(Path.Combine(installedLocation.FullName, VersionFileName), GetNewVersion());
}
