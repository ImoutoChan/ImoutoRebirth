using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Room.Application;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure;
using ImoutoRebirth.Room.UI;
using ImoutoRebirth.Room.UI.WebApi;

const string servicePrefix = "ROOM_";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWindowsService();
builder.SetWorkingDirectory();
builder.UseConfiguration<Program>(servicePrefix);
builder.ConfigureSerilog(
    (loggerBuilder, appConfiguration, hostEnvironment)
        => loggerBuilder
            .WithoutDefaultLoggers()
            .WithConsole()
            .WithAllRollingFile()
            .WithInformationRollingFile()
            .WithOpenSearch(appConfiguration, hostEnvironment));

var configuration = builder.Configuration;

builder.Services
    .AddRoomApplication(
        typeof(ImoutoRebirth.Room.Application.ServiceCollectionExtensions).Assembly,
        typeof(ImoutoRebirth.Room.DataAccess.ServiceCollectionExtensions).Assembly)
    .AddRoomDataAccess()
    .AddRoomDatabase(configuration.GetRequiredConnectionString("RoomDatabase"))
    .AddRoomInfrastructure()
    .AddRoomUi()
    .AddSqlMassTransit(builder.Configuration, "room", с => с.AddRoomMassTransitSetup())
    .AddWebEndpoints()
    .AddOpenTelemetry(builder.Environment, builder.Configuration);

var app = builder.Build();

app.MapWebEndpoints();

app.MigrateIfNecessary<RoomDbContext>();
app.Run();

public partial class Program;
