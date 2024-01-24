using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Meido.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddPostgresDbContext<MeidoDbContext>(connectionString);

        services.AddTransient<IUnitOfWork>(provider => provider.GetRequiredService<MeidoDbContext>());

        services.AddTransient<IParsingStatusRepository, ParsingStatusRepository>();
        services.AddTransient<ISourceActualizingStateRepository, SourceActualizingStateRepository>();
        
        return services;
    }

    public static IHost MigrateMeido(this IHost host) 
        => host.MigrateIfNecessary<MeidoDbContext>();
}
