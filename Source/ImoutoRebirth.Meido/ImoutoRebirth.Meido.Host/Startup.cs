using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Meido.Application;
using ImoutoRebirth.Meido.DataAccess;
using ImoutoRebirth.Meido.Host.Settings;
using ImoutoRebirth.Meido.Infrastructure;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Host;

public class Startup : BaseStartup
{
    public MeidoSettings MeidoSettings { get; }

    public Startup(IConfiguration configuration) : base(configuration) 
        => MeidoSettings = configuration.GetRequired<MeidoSettings>();

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddMeidoApplication()
            .AddMeidoUi()
            .ConfigureMeidoUi(Configuration)
            .AddMeidoDataAccess(Configuration.GetRequiredConnectionString("MeidoDatabase"))
            .AddMeidoInfrastructure();

        services.AddTrueMassTransit(
            MeidoSettings.RabbitSettings,
            MeidoReceiverApp.Name,
            с => с.AddMeidoServicesForRabbit());
    }
}
