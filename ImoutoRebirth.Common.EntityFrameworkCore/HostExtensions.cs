using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.EntityFrameworkCore
{
    public static class HostExtensions
    {
        public static IHost MigrateIfNecessary<TDbContext>(this IHost host) 
            where TDbContext : DbContext
        {
            var services = host.Services;

            var logger = services.GetRequiredService<ILogger>();
            var context = services.GetRequiredService<TDbContext>();

            context.Database.Migrate();

            var migrations = context.Database.GetAppliedMigrations();
            foreach (var migration in migrations)
                logger.LogInformation($"Migrated to {migration}");

            return host;
        }
    }
}
