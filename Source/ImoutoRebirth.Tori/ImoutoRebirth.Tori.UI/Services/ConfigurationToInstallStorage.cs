using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.Services;

namespace ImoutoRebirth.Tori.UI.Services;

public class ConfigurationToInstallStorage
{
    private readonly IMessenger _messenger;

    public ConfigurationToInstallStorage(IRegistryService registryService, IMessenger messenger)
    {
        _messenger = messenger;

        if (registryService.IsInstalled(out var installLocation))
        {
            var fileInInstallLocation = installLocation.GetFiles().FirstOrDefault(x => x.Name == ConfigurationService.ConfigurationFilename);

            if (fileInInstallLocation == null)
                return;

            CurrentConfiguration = AppConfiguration.ReadFromFile(fileInInstallLocation);
        }
    }

    public AppConfiguration? CurrentConfiguration { get; }
}
