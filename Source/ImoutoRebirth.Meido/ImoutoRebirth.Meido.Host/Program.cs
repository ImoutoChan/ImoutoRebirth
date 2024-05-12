using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.Application;
using ImoutoRebirth.Meido.DataAccess;
using ImoutoRebirth.Meido.Infrastructure;
using ImoutoRebirth.Meido.UI;
using ImoutoRebirth.Meido.UI.Consumers;
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

builder.Services
    .AddOpenTelemetry(builder.Environment, builder.Configuration)
    .AddMeidoApplication()
    .AddMeidoUi()
    .ConfigureMeidoUi(builder.Configuration)
    .AddMeidoDataAccess(builder.Configuration.GetRequiredConnectionString("MeidoDatabase"))
    .AddMeidoInfrastructure()
    .AddSqlMassTransit(builder.Configuration, "meido", с => с.AddMeidoMassTransitSetup(), typeof(NewFileCommandConsumer).Assembly);

await builder.Build()
    .MigrateMeido()
    .RunAsync();
