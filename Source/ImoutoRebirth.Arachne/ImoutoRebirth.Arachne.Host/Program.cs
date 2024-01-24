using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Host.Settings;
using ImoutoRebirth.Arachne.Infrastructure;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using Microsoft.Extensions.Hosting;

const string servicePrefix = "ARACHNE_";

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();
builder.SetWorkingDirectory();
builder.UseEnvironmentFromEnvironmentVariable(servicePrefix);
builder.UseConfiguration<Program>(servicePrefix);
builder.ConfigureSerilog(
    (loggerBuilder, appConfiguration, hostEnvironment)
        => loggerBuilder
            .WithoutDefaultLoggers()
            .WithConsole()
            .WithAllRollingFile()
            .WithInformationRollingFile()
            .WithOpenSearch(appConfiguration, hostEnvironment));

var arachneSettings = builder.Configuration.GetRequired<ArachneSettings>();

builder.Services
    .AddArachneCore()
    .AddArachneServices()
    .AddArachneInfrastructure(arachneSettings.DanbooruSettings, arachneSettings.SankakuSettings)
    .AddSqlMassTransit(builder.Configuration, "arachne", с => с.AddArachneMassTransitSetup(),
        typeof(EverywhereSearchMetadataCommandConsumer).Assembly)
    .AddOpenTelemetry(builder.Environment, builder.Configuration);

builder.Build().Run();


