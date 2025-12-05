using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddTransient(typeof(SoftDeleteDbContextHelper<>));
        services.AddPostgresDbContext<RoomDbContext>(
            connectionString,
            builder =>
            {
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                builder.MapEnum<ReportStatus>();
                builder.MapEnum<IntegrityStatus>();
            },
            dataSourceAction: builder =>
            {
                builder.MapEnum<ReportStatus>();
                builder.MapEnum<IntegrityStatus>();
            });
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RoomDbContext>());

        return services;
    }
}
