using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Host.Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Host
{
    public class RoomCompositionRoot
    {
        private IEnvironmentProvider _environment;
        private IConfiguration _configuration;
        private RoomAppSettings _settings;
        private ServiceProvider _serviceProvider;

        public void Init(IEnvironmentProvider environmentProvider, IConfiguration configuration)
        {
            _environment = environmentProvider;
            _configuration = configuration;
            _settings = configuration.Get<RoomAppSettings>();

            var services = new ServiceCollection();

            RegisterDependencies(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        private void RegisterDependencies(ServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(_configuration.GetConnectionString("RoomDatabase")));
        }
    }
}