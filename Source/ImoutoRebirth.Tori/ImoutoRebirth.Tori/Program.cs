﻿// See https://aka.ms/new-console-template for more information

using ImoutoRebirth.Tori;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddTransient<IConfigurationService, ConfigurationService>();
services.AddTransient<IRegistryService, RegistryService>();
services.AddTransient<IVersionService, VersionService>();

services.AddTransient<IWindowsServicesManager, WindowsServicesManager>();
//services.AddTransient<IWindowsServicesManager, FakeWindowsServicesManager>();

services.AddTransient<IWindowsServiceUpdater, WindowsServiceUpdater>();
services.AddTransient<IInstaller, Installer>();

services.AddLogging(builder => builder
    .ClearProviders()
    .AddProvider(new CustomConsoleLoggerProvider()));

var container = services.BuildServiceProvider();

var installer = container.GetRequiredService<IInstaller>();

var updaterLocation = new DirectoryInfo(args[0]);

if (!updaterLocation.Exists)
    Console.WriteLine("Unable to find updater location, please pass it as first argument");

installer.EasyInstallOrUpdate(updaterLocation);

Console.ReadKey();
