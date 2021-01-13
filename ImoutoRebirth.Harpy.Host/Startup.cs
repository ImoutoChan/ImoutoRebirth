using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Harpy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Harpy.Host
{
    internal class Startup : BaseStartup
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services) 
            => services.AddHarpyServices(Configuration);
    }
}
