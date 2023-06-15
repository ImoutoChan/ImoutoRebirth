using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.Application;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Host.Settings;
using ImoutoRebirth.Lilin.Infrastructure;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.WebApi;
using ImoutoRebirth.Lilin.WebApi.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

const string servicePrefix = "LILIN_";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWindowsService();
builder.SetWorkingDirectory();
builder.UseEnvironmentFromEnvironmentVariable(servicePrefix);
builder.UseConfiguration(servicePrefix);
builder.ConfigureSerilog(
    (loggerBuilder, appConfiguration, hostEnvironment)
        => loggerBuilder
            .WithoutDefaultLoggers()
            .WithConsole()
            .WithAllRollingFile()
            .WithInformationRollingFile()
            .WithOpenSearch(appConfiguration, hostEnvironment));

var lilinSettings = builder.Configuration.GetRequired<LilinSettings>();

builder.Services
    .AddLilinApplication(typeof(ImoutoRebirth.Lilin.Infrastructure.ServiceCollectionExtensions).Assembly)
    .AddLilinDataAccess(builder.Configuration.GetRequiredConnectionString("LilinDatabase"))
    .AddLilinInfrastructure()
    .AddLilinUi(builder.Configuration).AddTrueMassTransit(lilinSettings.RabbitSettings, ReceiverApp.Name, с => с.AddMassTransitUi())
    .AddQuartz()
    .AddWebEndpoints()
    .AddOpenTelemetry(builder.Environment, builder.Configuration);

var app = builder.Build();

app.MapWebEndpoints();

app.MigrateIfNecessary<LilinDbContext>();
app.Run();
