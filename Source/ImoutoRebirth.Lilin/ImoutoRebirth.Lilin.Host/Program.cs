using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.Core;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Host.Settings;
using ImoutoRebirth.Lilin.Infrastructure;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services;
using ImoutoRebirth.Lilin.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
    .AddLilinInfrastructure()
    .AddLilinServices(builder.Configuration)
    .AddLilinDataAccess(builder.Configuration.GetConnectionString("LilinDatabase")
                        ?? throw new Exception("ConnectionString is empty"))
    .AddLilinCore();

builder.Services
    .AddTrueMassTransit(
        lilinSettings.RabbitSettings,
        ReceiverApp.Name,
        с => с.AddLilinServicesForRabbit());

builder.Services.AddQuartz();

builder.Services.ConfigureWebApp();

var app = builder.Build();

app.UseWebApp();

app.MigrateIfNecessary<LilinDbContext>();
app.Run();
