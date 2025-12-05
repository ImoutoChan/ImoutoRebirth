using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace ImoutoRebirth.Common.EntityFrameworkCore;

internal static class NpgsqlNodaTimeDbContextOptionsBuilderExtensions
{
    public static NpgsqlDbContextOptionsBuilder UseEfNodaTime(
        this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

#pragma warning disable EF1001
        var extension = coreOptionsBuilder.Options.FindExtension<NpgsqlNodaTimeOptionsExtension>()
                        ?? new NpgsqlNodaTimeOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
#pragma warning restore EF1001
    }
}

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        Action<NpgsqlDataSourceBuilder>? dataSourceAction = null)
        where TContext : DbContext
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        dataSourceAction?.Invoke(dataSourceBuilder);
        var dataSource = dataSourceBuilder.Build();
        
        services.AddDbContext<TContext>(o => o.UseNpgsql(dataSource, builder =>
        {
            builder.UseEfNodaTime();
            npgsqlOptionsAction?.Invoke(builder);
        }));

        return services;
    }
}
