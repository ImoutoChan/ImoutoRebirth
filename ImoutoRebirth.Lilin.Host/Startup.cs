using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Lilin.DataAccess;
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
            services.AddDbContext<LilinDbContext>(o 
                => o.UseNpgsql(Configuration.GetConnectionString("LilinDatabase")));
        }
    }
}