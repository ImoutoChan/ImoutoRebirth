using System.Text.Json;
using ImoutoRebirth.Tori.Services;

namespace ImoutoRebirth.Tori.Tests;

public class PackageValidationTests : IDisposable
{
    private readonly DirectoryInfo _package = Directory.CreateTempSubdirectory();

    [Fact]
    public void MissingRoomExecutableRejectsPackage()
    {
        CreateLegacyPackage();
        File.Delete(Path.Combine(_package.FullName, "ImoutoRebirth.Room", "ImoutoRebirth.Room.Host.exe"));

        Assert.Throws<InvalidDataException>(() => WindowsServiceUpdater.ValidatePackage(_package));
    }

    [Fact]
    public void EmptyNavigatorAssemblyRejectsLegacyPackage()
    {
        CreateLegacyPackage();
        File.WriteAllBytes(Path.Combine(_package.FullName, "ImoutoRebirth.Navigator", "ImoutoRebirth.Navigator.dll"), []);

        Assert.Throws<InvalidDataException>(() => WindowsServiceUpdater.ValidatePackage(_package));
    }

    [Fact]
    public void InventoryDetectsTruncatedDependency()
    {
        CreateLegacyPackage();
        const string library = "ImoutoRebirth.Room/dependency.dll";
        File.WriteAllBytes(Path.Combine(_package.FullName, library), [1]);
        var inventory = JsonSerializer.Serialize(new Dictionary<string, long> { [library] = 2 });
        File.WriteAllText(Path.Combine(_package.FullName, "package-files.json"), inventory);

        Assert.Throws<InvalidDataException>(() => WindowsServiceUpdater.ValidatePackage(_package));
    }

    [Fact]
    public void CompleteLegacyPackagePassesValidation()
    {
        CreateLegacyPackage();

        WindowsServiceUpdater.ValidatePackage(_package);
    }

    private void CreateLegacyPackage()
    {
        foreach (var name in new[] { "Room", "Lilin", "Arachne", "Harpy", "Kekkai", "Lamia", "Meido", "Navigator" })
        {
            var service = "ImoutoRebirth." + name;
            var assembly = name is "Kekkai" or "Navigator" ? service : service + ".Host";
            var directory = _package.CreateSubdirectory(service);
            foreach (var suffix in new[] { ".exe", ".dll", ".deps.json", ".runtimeconfig.json" })
                File.WriteAllBytes(Path.Combine(directory.FullName, assembly + suffix), [1]);
            if (name != "Navigator")
                File.WriteAllText(Path.Combine(directory.FullName, "appsettings.Production.json"), "{}");
        }

        var viewer = _package.CreateSubdirectory("Imouto.Viewer");
        File.WriteAllBytes(Path.Combine(viewer.FullName, "ImoutoViewer.exe"), [1]);
    }

    public void Dispose() => _package.Delete(recursive: true);
}
