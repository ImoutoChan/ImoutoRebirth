using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Harpy.Services;

namespace ImoutoRebirth.Harpy.Host;

internal class Startup : BaseStartup
{
    public Startup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
        => services.AddHarpyServices(Configuration);
}