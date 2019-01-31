using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.Core;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Host.Settings;
using ImoutoRebirth.Lilin.Infrastructure;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Host
{
    public class Startup : BaseStartup
    {
        public LilinSettings LilinSettings { get; }

        public Startup(IConfiguration configuration)
            : base(configuration)
        {
            LilinSettings = configuration.Get<LilinSettings>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LilinDbContext>(
                o => o.UseNpgsql(Configuration.GetConnectionString("LilinDatabase")));
            
            services.AddLilinInfrastructure()
                    .AddLilinServices()
                    .AddLilinDataAccess()
                    .AddLilinCore();

            services.AddTrueMassTransit(
                LilinSettings.RabbitSettings,
                ReceiverApp.Name,
                с => с.AddLilinServicesForRabbit());
        }
    }
}