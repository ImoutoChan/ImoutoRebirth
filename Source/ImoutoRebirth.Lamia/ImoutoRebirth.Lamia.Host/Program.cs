using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Lamia.Application;
using ImoutoRebirth.Lamia.Infrastructure;
using ImoutoRebirth.Lamia.UI;
using Microsoft.Extensions.Hosting;

const string servicePrefix = "LAMIA_";

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
builder.ConfigureOpenTelemetryLogging();

var configuration = builder.Configuration;

builder.Services
    .AddOpenTelemetry(builder.Environment, builder.Configuration)
    .AddLamiaApplication(
        typeof(ImoutoRebirth.Lamia.Application.ServiceCollectionExtensions).Assembly)
    .AddLamiaInfrastructure(configuration)
    .AddSqlMassTransit(
        builder.Configuration,
        "lamia",
        с => с.AddLamiaMassTransitSetup(),
        true,
        typeof(ExtractFileMetadataCommandConsumer).Assembly);

var app = builder.Build();

app.Run();

public partial class Program;
