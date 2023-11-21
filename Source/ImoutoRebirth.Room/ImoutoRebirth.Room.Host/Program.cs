using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Room.Application;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Host;
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
var roomSettings = builder.Configuration.GetRequired<RoomSettings>();

builder.Services
    .AddRoomApplication(typeof(ImoutoRebirth.Room.Application.ServiceCollectionExtensions).Assembly)
    .AddRoomDataAccess()
    .AddRoomDatabase(configuration)
    .AddRoomInfrastructure()
    .AddRoomUi()
    .AddTrueMassTransit(roomSettings.RabbitSettings, "ImoutoRebirth.Room", с => с.AddRoomInfrastructureForRabbit())
    .AddQuartz()
    .AddWebEndpoints()
    .AddOpenTelemetry(builder.Environment, builder.Configuration);

var app = builder.Build();

app.MapWebEndpoints();

app.MigrateIfNecessary<RoomDbContext>();
app.Run();

public partial class Program;
