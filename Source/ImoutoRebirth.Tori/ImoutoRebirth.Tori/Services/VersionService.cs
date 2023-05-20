namespace ImoutoRebirth.Tori.Services;

public interface IVersionService
{
    string GetNewVersion();

    string GetLocalVersion(DirectoryInfo installedLocation);

    void SetLocalVersionAsNew(DirectoryInfo installedLocation);
}

public class VersionService : IVersionService
{
    private const string VersionFileName = "version.txt";

    public string GetNewVersion() => typeof(VersionService).Assembly.GetName().Version!.ToString();

    public string GetLocalVersion(DirectoryInfo installedLocation)
    {
        var versionFile = installedLocation.GetFiles().FirstOrDefault(x => x.Name == VersionFileName);

        if (versionFile?.Exists != true)
            return "0.0.0";

        return File.ReadAllText(versionFile.FullName);
    }

    public void SetLocalVersionAsNew(DirectoryInfo installedLocation)
        => File.WriteAllText(Path.Combine(installedLocation.FullName, VersionFileName), GetNewVersion());
}