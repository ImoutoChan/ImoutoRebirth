using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.DataAccess;
using ImoutoRebirth.Meido.Host.Settings;
using ImoutoRebirth.Meido.Infrastructure;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Host
{
    public class Startup : BaseStartup
    {
        public MeidoSettings MeidoSettings { get; }

        public Startup(IConfiguration configuration)
            : base(configuration)
        {
            MeidoSettings = configuration.Get<MeidoSettings>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMeidoServices()
                    .ConfigureMeidoServices(Configuration)
                    .AddMeidoDataAccess(Configuration.GetConnectionString("MeidoDatabase"))
                    .AddMeidoDomain()
                    .AddMeidoInfrastructure();

            services.AddTrueMassTransit(
                MeidoSettings.RabbitSettings,
                ReceiverApp.Name,
                с => с.AddMeidoServicesForRabbit());
        }
    }
}