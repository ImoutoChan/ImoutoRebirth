using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Harpy.Services;
using GenericHost = Microsoft.Extensions.Hosting.Host;

const string servicePrefix = "HARPY_";

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
    .AddHarpyServices(builder.Configuration)
    .AddOpenTelemetry(builder.Environment, builder.Configuration);

builder.Build().Run();
