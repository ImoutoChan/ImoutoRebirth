using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Host.Settings;
using ImoutoRebirth.Arachne.Infrastructure;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Common.Host;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Host
{
    public class Startup : BaseStartup
    {
        public ArachneSettings ArachneSettings { get; }

        public Startup(IConfiguration configuration) 
            : base(configuration)
        {
            ArachneSettings = configuration.Get<ArachneSettings>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddArachneCore()
                    .AddArachneServices()
                    .AddArachneInfrastructure(ArachneSettings.DanbooruSettings, ArachneSettings.SankakuSettings);

            services.AddMassTransitRabbitMqHostedService(ReceiverApp.Name, ArachneSettings.RabbitSettings.ToOptions())
                    .AddArachneServicesForRabbit();
        }
    }
}