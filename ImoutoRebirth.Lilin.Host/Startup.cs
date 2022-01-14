using AutoMapper;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.Core;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Host.Settings;
using ImoutoRebirth.Lilin.Infrastructure;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services;
using ImoutoRebirth.Lilin.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Host;

public class Startup : BaseStartup
{
    private LilinSettings LilinSettings { get; }

    public Startup(IConfiguration configuration) 
        : base(configuration)
    {
        LilinSettings = configuration.Get<LilinSettings>();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLilinInfrastructure()
            .AddLilinServices(Configuration)
            .AddLilinDataAccess(Configuration.GetConnectionString("LilinDatabase"))
            .AddLilinCore();

        services
            .AddAutoMapper(typeof(DtoAutoMapperProfile))
            .AddTrueMassTransit(
                LilinSettings.RabbitSettings,
                ReceiverApp.Name,
                с => с.AddLilinServicesForRabbit());
    }

    public void Configure(IMapper mapper)
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}