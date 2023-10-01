using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.Application;
using ImoutoRebirth.Meido.DataAccess;
using ImoutoRebirth.Meido.Host;
using ImoutoRebirth.Meido.Infrastructure;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.UI;
using Microsoft.Extensions.Hosting;

const string servicePrefix = "MEIDO_";

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
builder.UseQuartz();

var meidoSettings = builder.Configuration.GetRequired<MeidoSettings>();

builder.Services
    .AddOpenTelemetry(builder.Environment, builder.Configuration)
    .AddMeidoApplication()
    .AddMeidoUi()
    .ConfigureMeidoUi(builder.Configuration)
    .AddMeidoDataAccess(builder.Configuration.GetRequiredConnectionString("MeidoDatabase"))
    .AddMeidoInfrastructure();

builder.Services.AddTrueMassTransit(
    meidoSettings.RabbitSettings,
    MeidoReceiverApp.Name,
    с => с.AddMeidoServicesForRabbit());

await builder.Build()
    .MigrateMeido()
    .RunAsync();
