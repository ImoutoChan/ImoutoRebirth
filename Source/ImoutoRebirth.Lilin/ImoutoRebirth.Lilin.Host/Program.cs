using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.Application;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure;
using ImoutoRebirth.Lilin.UI;
using ImoutoRebirth.Lilin.UI.Consumers;
using ImoutoRebirth.Lilin.UI.WebApi;

const string servicePrefix = "LILIN_";

var builder = WebApplication.CreateBuilder(args);

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

builder.Services
    .AddOpenTelemetry(builder.Environment, builder.Configuration)
    .AddLilinApplication(typeof(ImoutoRebirth.Lilin.Infrastructure.ServiceCollectionExtensions).Assembly)
    .AddLilinDataAccess(builder.Configuration.GetRequiredConnectionString("LilinDatabase"))
    .AddLilinInfrastructure()
    .AddLilinUi(builder.Configuration)
    .AddSqlMassTransit(builder.Configuration, "lilin", с => с.AddLilinMassTransitSetup(), true,
        typeof(UpdateMetadataCommandConsumer).Assembly)
    .AddQuartz()
    .AddWebEndpoints();

var app = builder.Build();

app.MapWebEndpoints();
app.MigrateIfNecessary<LilinDbContext>();
app.Run();

public partial class Program;
