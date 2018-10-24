using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Host.Settings;
using ImoutoRebirth.Arachne.Infrastructure;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public ArachneSettings ArachneSettings { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            ArachneSettings = configuration.Get<ArachneSettings>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddArachneCore()
                    .AddArachneInfrastructure(ArachneSettings.DanbooruSettings, ArachneSettings.SankakuSettings);

            services.AddMassTransitRabbitMqHostedService(ReceiverApp.Name, ArachneSettings.RabbitSettings.ToOptions())
                    .AddSearchMetadataCommandConsumer();
        }
    }
}