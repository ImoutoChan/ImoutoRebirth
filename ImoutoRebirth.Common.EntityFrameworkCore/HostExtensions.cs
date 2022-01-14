using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.EntityFrameworkCore;

public static class HostExtensions
{
    public static IHost MigrateIfNecessary<TDbContext>(this IHost host) 
        where TDbContext : DbContext
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<TDbContext>>();
        var context = services.GetRequiredService<TDbContext>();

        context.Database.Migrate();

        var migrations = context.Database.GetAppliedMigrations();
        foreach (var migration in migrations)
            logger.LogInformation("Migrated to {Migration}", migration);

        return host;
    }
}