using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Meido.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MeidoDbContext>(
            o => o.UseNpgsql(connectionString, builder => builder.UseNodaTime()));

        services.AddTransient<IUnitOfWork>(provider => provider.GetRequiredService<MeidoDbContext>());

        services.AddTransient<IParsingStatusRepository, ParsingStatusRepository>();
        services.AddTransient<ISourceActualizingStateRepository, SourceActualizingStateRepository>();
        
        return services;
    }

    public static IHost MigrateMeido(this IHost host) 
        => host.MigrateIfNecessary<MeidoDbContext>();
}
