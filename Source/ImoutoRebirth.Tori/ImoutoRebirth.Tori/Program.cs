// See https://aka.ms/new-console-template for more information

using System.Reflection;
using ImoutoRebirth.Tori;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddTransient<IConfigurationService, ConfigurationService>();
services.AddTransient<IRegistryService, RegistryService>();
services.AddTransient<IVersionService, VersionService>();
services.AddTransient<IWindowsServicesManager, WindowsServicesManager>();
services.AddTransient<IWindowsServiceUpdater, WindowsServiceUpdater>();
services.AddTransient<IInstaller, Installer>();

services.AddLogging(builder => builder.AddConsole());

var container = services.BuildServiceProvider();

var installer = container.GetRequiredService<IInstaller>();

var updaterLocation =  
    new DirectoryInfo(Assembly.GetEntryAssembly()!.Location).Parent!;

installer.EasyInstallOrUpdate(updaterLocation);
