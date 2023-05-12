using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.Host;

public abstract class BaseStartup
{
    public IConfiguration Configuration { get; }

    protected BaseStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public abstract void ConfigureServices(IServiceCollection services);
}