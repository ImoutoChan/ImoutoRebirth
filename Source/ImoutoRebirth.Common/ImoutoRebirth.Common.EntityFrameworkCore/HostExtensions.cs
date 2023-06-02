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

        
        var beforeMigrations = context.Database.GetAppliedMigrations().ToHashSet();
        
        context.Database.Migrate();

        var afterMigrations = context.Database.GetAppliedMigrations();
        
        foreach (var migration in afterMigrations)
        {
            var logMessage = beforeMigrations.Contains(migration)
                ? "Migration {Migration} was applied earlier"
                : "Migration {Migration} was applied just now";
            
            logger.LogInformation(logMessage, migration);
        }

        return host;
    }
}
