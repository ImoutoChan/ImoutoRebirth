using AutoMapper;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Room.Core;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure;
using ImoutoRebirth.Room.WebApi;
using ImoutoRebirth.Room.Webhost.Quartz;
using ImoutoRebirth.Room.Webhost.Settings;

namespace ImoutoRebirth.Room.Webhost;

public class Startup
{
    public RoomAppSettings RoomSettings { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        RoomSettings = configuration.Get<RoomAppSettings>() ?? throw new Exception("Wrong configuration");
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRoomServices();
        services.AddRoomCore();
        services.AddRoomDataAccess();
        services.AddRoomDatabase(Configuration);
        services.AddRoomWebApi();

        services.AddTrueMassTransit(
            RoomSettings.RabbitSettings,
            "ImoutoRebirth.Room",
            с => с.AddRoomServicesForRabbit());

        services.AddQuartzJob<OverseeJob, OverseeJob.Description>();

        services.AddAutoMapper(typeof(ModelAutoMapperProfile), typeof(DtoAutoMapperProfile));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();

        app.ConfigureWebApi(env);
    }
}
